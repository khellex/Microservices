using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;
        private ILogger<CartAPIController> _logger;

        public CartAPIController(ApplicationDbContext db, IMapper mapper, ILogger<CartAPIController> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _response = new();
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
        [HttpPost("RemoveCart")]
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
    }
}
