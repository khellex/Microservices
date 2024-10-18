namespace Mango.Web.Models
{
    /// <summary>
    /// The user will send the following details to request login
    /// We will use this Dto to handle that
    /// </summary>
    public class LoginRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
