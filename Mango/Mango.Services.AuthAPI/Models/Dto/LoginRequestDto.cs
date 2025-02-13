using System.ComponentModel.DataAnnotations;

namespace Mango.Services.AuthAPI.Models.Dto
{
    /// <summary>
    /// The user will send the following details to request login
    /// We will use this Dto to handle that
    /// </summary>
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
