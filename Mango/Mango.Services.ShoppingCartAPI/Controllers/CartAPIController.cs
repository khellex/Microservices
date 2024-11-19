using AutoMapper;
using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.RabbitMQMessageSender;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    [Authorize]
    public class CartAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;
        private ILogger<CartAPIController> _logger;
        private readonly IProductService _productService;
        private readonly ICouponService _couponService;
        private readonly IMessageBus _messageBus;
        private readonly IConfiguration _configuration;
        private readonly IRabbitMQCartMessageSender _rabbitMQAuthMessageSender;

        public CartAPIController(ApplicationDbContext db, IMapper mapper, ILogger<CartAPIController> logger, IProductService productService, ICouponService couponService,
            IMessageBus messageBus, IConfiguration configuration, IRabbitMQCartMessageSender rabbitMQAuthMessageSender)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _response = new();
            _productService = productService;
            _couponService = couponService;
            _messageBus = messageBus;
            _configuration = configuration;
            _rabbitMQAuthMessageSender = rabbitMQAuthMessageSender;
        }
        /// <summary>
        /// Controller method to add/edit Shopping cart for a user
        /// </summary>
        /// <param name="cartDto"></param>
        /// <returns></returns>
        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> CartUpsert(CartDto cartDto)
        {
            try
            {
                //check if the user cart exists
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(h => h.UserId == cartDto.CartHeaderDto.UserId);
                if (cartHeaderFromDb == null)
                {
                    //if cart does not exist, add new cart for this user
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeaderDto);
                    await _db.CartHeaders.AddAsync(cartHeader);
                    await _db.SaveChangesAsync();

                    //after adding the new cart, the cartHeaderId is sent to the cart details table
                    cartDto.CartDetailsDto.First().CartHeaderId = cartHeader.CartHeaderId; //fulfilling the FK constraint
                    await _db.CartDetails.AddAsync(_mapper.Map<CartDetails>(cartDto.CartDetailsDto.First()));
                    await _db.SaveChangesAsync();

                    _response.Message = "Cart added successfully.";
                }
                else
                {
                    //cart exists, check if the product is previously added
                    var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                        d => d.ProductId == cartDto.CartDetailsDto.First().ProductId &&
                        d.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                    cartDto.CartDetailsDto.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;  //fulfilling the FK constraint

                    if (cartDetailsFromDb == null)
                    {
                        //add new product to the cart
                        await _db.CartDetails.AddAsync(_mapper.Map<CartDetails>(cartDto.CartDetailsDto.First()));
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        //if added, then increase count of same product
                        cartDto.CartDetailsDto.First().Count += cartDetailsFromDb.Count;
                        cartDto.CartDetailsDto.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                        _db.CartDetails.Update(_mapper.Map<CartDetails>(cartDto.CartDetailsDto.First()));
                        await _db.SaveChangesAsync();
                    }
                    _response.Message = "Cart updated successfully.";
                }
                _response.Result = cartDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        /// <summary>
        /// Controller method to delete the cart header and details
        /// </summary>
        /// <param name="cartDetailsID"></param>
        /// <returns></returns>
        [HttpDelete("RemoveCart")]
        public async Task<ResponseDto> RemoveCart([FromBody] int cartDetailsID)
        {
            try
            {
                CartDetails? cartDetails = await _db.CartDetails.FirstOrDefaultAsync(d => d.CartDetailsId == cartDetailsID);
                if (cartDetails != null)
                {
                    var totalCountOfCartItem = await _db.CartDetails.CountAsync(d => d.CartHeaderId == cartDetails.CartHeaderId);
                    if (totalCountOfCartItem == 1)
                    {
                        //delete the cart details as well as the header
                        var cartHeaderFromDb = await _db.CartHeaders.FirstOrDefaultAsync(h => h.CartHeaderId == cartDetails.CartHeaderId);
                        if (cartHeaderFromDb != null)
                        {
                            _db.CartHeaders.Remove(cartHeaderFromDb);
                        }
                    }
                    _db.CartDetails.Remove(cartDetails);
                    await _db.SaveChangesAsync();

                    _response.Result = true;
                    _response.Message = "Cart deleted successfully.";
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Message = "No entries to be deleted.";
                }
                return _response;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        /// <summary>
        /// Gets the full cart header+details for the user
        /// Also, returns the full details of products 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCart(string userId)
        {
            try
            {
                var checkIfUserCartExists = await _db.CartHeaders.FirstOrDefaultAsync(h => h.UserId == userId);

                if (checkIfUserCartExists != null)
                {
                    var userCartHasProducts = await _db.CartDetails.Where(d => d.CartHeaderId == checkIfUserCartExists.CartHeaderId).ToListAsync();

                    if (userCartHasProducts != null)
                    {
                        CartDto cart = new()
                        {
                            CartHeaderDto = _mapper.Map<CartHeaderDto>(checkIfUserCartExists),
                            CartDetailsDto = _mapper.Map<IEnumerable<CartDetailsDto>>(userCartHasProducts),
                        };
                        //making a call to the product API to fetch all the products
                        IEnumerable<ProductDto> products = await _productService.GetProductAsync();

                        foreach (var item in cart.CartDetailsDto)
                        {
                            item.ProductDto = products.FirstOrDefault(p => p.ProductId == item.ProductId);
                            cart.CartHeaderDto.CartTotal += (item.Count * item.ProductDto.Price);
                        }

                        if (!string.IsNullOrEmpty(cart.CartHeaderDto.CouponCode))
                        {
                            //making a call to the coupon API to fetch the coupon based on coupon code
                            CouponDto coupon = await _couponService.GetCouponAsync(cart.CartHeaderDto.CouponCode);
                            if (coupon != null && cart.CartHeaderDto.CartTotal > coupon.MinAmount)
                            {
                                cart.CartHeaderDto.CartTotal -= coupon.DiscountAmount;
                                cart.CartHeaderDto.Discount = coupon.DiscountAmount;
                            }
                        }
                        _response.Result = cart;
                    }
                    else
                    {
                        _response.IsSuccess = false;
                        _response.Message = "User cart does not have any products.";
                    }
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Message = "User cart does not exist.";
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        [HttpPost("ApplyCoupon")]
        public async Task<ResponseDto> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var checkUserCartExists = await _db.CartHeaders.FirstOrDefaultAsync(h => h.UserId == cartDto.CartHeaderDto.UserId);
                if (checkUserCartExists != null && cartDto.CartHeaderDto.CouponCode != null)
                {
                    var checkIfCouponCodeIsValid = await _couponService.GetCouponAsync(cartDto.CartHeaderDto.CouponCode);
                    //coupon code is returned null if the coupon is invalid
                    if (checkIfCouponCodeIsValid.CouponCode != null)
                    {
                        checkUserCartExists.CouponCode = cartDto.CartHeaderDto.CouponCode;
                        _db.Update(checkUserCartExists);
                        await _db.SaveChangesAsync();
                        _response.Result = checkUserCartExists;
                        _response.Message = "Coupon applied successfully.";
                    }
                    else
                    {
                        _response.Message = "Invalid coupon code.";
                        _response.IsSuccess = false;
                    }
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Message = "User cart does not exist.";
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        [HttpPost("RemoveCoupon")]
        public async Task<ResponseDto> RemoveCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var checkUserCartExists = await _db.CartHeaders.FirstOrDefaultAsync(h => h.UserId == cartDto.CartHeaderDto.UserId);
                if (checkUserCartExists != null)
                {
                    checkUserCartExists.CouponCode = string.Empty;
                    _db.Update(checkUserCartExists);
                    await _db.SaveChangesAsync();
                    _response.Result = checkUserCartExists;
                    _response.Message = "Coupon code removed successfully.";
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Message = "User cart does not exist.";
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        [HttpPost("EmailCartRequest")]
        public async Task<ResponseDto> EmailCartRequest([FromBody] CartDto cartDto)
        {
            try
            {
                var checkUserCartExists = await _db.CartHeaders.FirstOrDefaultAsync(h => h.UserId == cartDto.CartHeaderDto.UserId);
                if (checkUserCartExists != null)
                {
                    //await _messageBus.PublishMessage(cartDto, _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"));

                    //sending the message tot he rabbit mq instance
                    _rabbitMQAuthMessageSender.SendMessage(cartDto, _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"));

                    _response.Message = "Email will be processed and sent successfully.";
                    _response.Result = true;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}
