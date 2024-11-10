using AutoMapper;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using static Mango.Services.OrderAPI.Utility.StaticDetails;

namespace Mango.Services.OrderAPI.Controller
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPI : ControllerBase
    {
        private IMapper _mapper;
        private ResponseDto _response;
        private readonly ApplicationDbContext _db;
        private readonly IProductService _productService;

        public OrderAPI(IMapper mapper, ApplicationDbContext db, IProductService productService)
        {
            _mapper = mapper;
            _db = db;
            _productService = productService;
            _response = new();
        }
        [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> CreateOrder([FromBody] CartDto cart)
        {
            try
            {
                //we map the incoming cart dto to the order header dto
                OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cart.CartHeaderDto);
                orderHeaderDto.OrderTime = DateTime.Now;
                orderHeaderDto.Status = Statuses[OrderStatus.Pending];

                //mapping the cart details dto to the order details dto inside the order header dto
                orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cart.CartDetailsDto);

                OrderHeader orderCreated = (await _db.OrderHeaders.AddAsync(_mapper.Map<OrderHeader>(orderHeaderDto))).Entity;
                await _db.SaveChangesAsync();

                orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
                _response.Result = orderHeaderDto;
                _response.Message = "Order Created successfully";
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [Authorize]
        [HttpPost("CreateStripeSession")]
        public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
        {
            try
            {
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = "https://example.com/success",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                    {
                        new Stripe.Checkout.SessionLineItemOptions
                        {
                            Price = "price_1MotwRLkdIwHu7ixYcPLm5uZ",
                            Quantity = 2,
                        },
                    },
                    Mode = "payment",
                };
                var service = new Stripe.Checkout.SessionService();
                service.Create(options);
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message;
                _response.IsSuccess = false;
                throw;
            }
            return _response;
        }
    }
}
