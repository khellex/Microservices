using Microsoft.AspNetCore.Identity;

namespace Mango.Services.AuthAPI.Models
{
    /// <summary>
    /// This class is used to add additional
    /// properties to the AspNetUsers table for identity.
    /// We are using this class to add the Name property.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
