using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models;
public class ProductDto
{
    public int ProductId { get; set; }
    [Range(1.0, 100.0, ErrorMessage = "Price must be between 1 and 100.")]
    public double Price { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string CategoryName { get; set; }
    public string? ImageUrl { get; set; }
    [Range(1,10)]
    public int Count { get; set; } = 1;
    public string? ImageLocalPath { get; set; }
    public IFormFile? Image { get; set; }
}
