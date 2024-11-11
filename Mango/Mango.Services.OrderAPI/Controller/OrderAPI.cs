using AutoMapper;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
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
                //options for the strip checkout initiation
                var options = new SessionCreateOptions
                {
                    SuccessUrl = stripeRequestDto.ApprovedUrl, //if the checkout is success, then where should stripe redirect after that
                    CancelUrl = stripeRequestDto.CancelUrl, //if the checkout is failed/cancelled, then where should stripe redirect after that
                    LineItems = new List<SessionLineItemOptions>(), //all the checkout screen contents
                    Mode = "payment",
                };

                var discountObj = new List<SessionDiscountOptions>()
                {
                    new SessionDiscountOptions()
                    {
                        Coupon=stripeRequestDto.OrderHeaderDto.CouponCode.ToUpper(),
                    }
                };

                foreach (var item in stripeRequestDto.OrderHeaderDto.OrderDetails)
                {
                    var sessionLineItem = new SessionLineItemOptions()
                    {
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            UnitAmount = (long)(item.Price * 100),  //₹20.99 => 2099
                            Currency = "inr",
                            ProductData = new SessionLineItemPriceDataProductDataOptions()
                            {
                                Name = item.ProductDto.Name,
                            }
                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                //add coupon only if the discount is more than 0
                if (stripeRequestDto.OrderHeaderDto.Discount > 0)
                {
                    options.Discounts = discountObj;
                }
                //creates a stripe session service
                var service = new SessionService();

                //initialize a stripe session object, so we can fetch the stripe id and session URL for future purposes 
                Session session = await service.CreateAsync(options);
                stripeRequestDto.StripeSessionUrl = session.Url;

                OrderHeader orderHeader = await _db.OrderHeaders.FirstOrDefaultAsync(h => h.OrderHeaderId == stripeRequestDto.OrderHeaderDto.OrderHeaderId);
                if (orderHeader != null)
                {
                    orderHeader.StripeSessionId = session.Id;
                    await _db.SaveChangesAsync();
                    _response.Result = stripeRequestDto;
                    _response.Message = "Payment initiated successfully.";
                }
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message;
                _response.IsSuccess = false;
            }
            return _response;
        }
        [Authorize]
        [HttpPost("ValidateStripeSession")]
        public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
        {
            try
            {
                OrderHeader orderHeader = await _db.OrderHeaders.FirstOrDefaultAsync(h => h.OrderHeaderId == orderHeaderId);

                //creates a stripe session service
                var service = new SessionService();

                //initialize a stripe session object, so we can fetch the stripe id and session URL for future purposes 
                Session session = await service.GetAsync(orderHeader.StripeSessionId);

                //create a payment intent service object
                var paymentIntentService = new PaymentIntentService();

                //access the payment service object to check the payment status
                PaymentIntent paymentIntent = await paymentIntentService.GetAsync(session.PaymentIntentId);

                if (paymentIntent.Status == "succeeded")
                {
                    //when the payment was successful, as per the status fetched from Stripe
                    orderHeader.PaymentIntentId = paymentIntent.Id;
                    orderHeader.Status = Statuses[OrderStatus.Approved];
                    await _db.SaveChangesAsync();

                    _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                    _response.Message = "Payment validated successfully.";
                }
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message;
                _response.IsSuccess = false;
            }
            return _response;
        }

    }
}
