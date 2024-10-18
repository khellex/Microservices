namespace Mango.Web.Models
{
    /// <summary>
    /// This Dto will be sent as a 
    /// response after the user successfully logs in.
    /// Here we will use the UserDto details and
    /// the JWT in the response to authenticate the user
    /// </summary>
    public class LoginResponseDto
    {
        public UserDto? User { get; set; }
        public string Token { get; set; }
    }
}
