namespace Mango.Web.Models
{
    /// <summary>
    /// This Dto will be returned to the successfully logged in user.
    /// </summary>
    public class UserDto
    {
        //here the Id is string because we will
        //be using the Guid for the user Id
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
