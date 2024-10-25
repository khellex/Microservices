using Mango.Web.Models;
using Mango.Web.Service;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        #region Product Services
        /// <summary>
        /// Fetches the full list of products using the 
        /// GetAllProductsAsync() api endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ProductIndex()
        {
            List<ProductDto?> product = new();

            ResponseDto? response = await _productService.GetAllProductsAsync();
            if (response != null && response.IsSuccess)
            {
                product = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response.Message;
            }
            return View(product);
        }
        /// <summary>
        /// Used to load the initial create Product page view
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CreateProduct()
        {
            return View();
        }
        /// <summary>
        /// Used to create new product by calling the 
        /// CreateProductsAsync() api endpoint
        /// </summary>
        /// <param name="productDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductDto productDto)
        {
            if (ModelState.IsValid)
            {
                ResponseDto? response = await _productService.CreateProductsAsync(productDto);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = response.Message;
                    return RedirectToAction(nameof(ProductIndex));
                    
                }
            }
            TempData["error"] = "Something went wrong";
            return View(productDto);
        }
        /// <summary>
        /// Used to load the initial Delete product page
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            ResponseDto? response = await _productService.GetProductByIdAsync(productId);
            if (response != null && response.IsSuccess)
            {
                ProductDto? product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
                return View(product);
            }
            TempData["error"] = "Something went wrong";
            return NotFound();
        }
        /// <summary>
        /// Used to delete a product from the listing using the 
        /// DeleteProductsAsync() api endpoint and then redirect
        /// the page to the Product index page.
        /// </summary>
        /// <param name="ProductDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(ProductDto productDto)
        {
            var deleteProduct = await _productService.DeleteProductsAsync(productDto.ProductId);

            ResponseDto? response = await _productService.GetAllProductsAsync();

            if (deleteProduct != null && deleteProduct.IsSuccess)
            {
                TempData["success"] = deleteProduct.Message;
                return RedirectToAction(nameof(ProductIndex), response);
            }
            return RedirectToAction(nameof(DeleteProduct), productDto.ProductId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditProduct(int productId)
        {
            ResponseDto? response = await _productService.GetProductByIdAsync(productId);
            if (response != null && response.IsSuccess)
            {
                ProductDto? product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
                return View(product);
            }
            TempData["error"] = "Something went wrong";
            return NotFound();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductDto productDto)
        {
            var editProduct = await _productService.UpdateProductsAsync(productDto);

            ResponseDto? response = await _productService.GetAllProductsAsync();

            if (editProduct != null && editProduct.IsSuccess)
            {
                TempData["success"] = editProduct.Message;
                return RedirectToAction(nameof(ProductIndex), response);
            }
            return RedirectToAction(nameof(EditProduct), productDto.ProductId);
        }
        #endregion
    }
}
