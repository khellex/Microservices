using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models
{
    /// <summary>
    /// This Dto will be used when a new user
    /// wants to register on the application.
    /// The user will share the following details for registration
    /// </summary>
    public class RegistrationRequestDto
    {
        [Required]
        public string Password { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public string? Role { get; set; }
    }
}
