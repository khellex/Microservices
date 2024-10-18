﻿namespace Mango.Web.Models
{
    /// <summary>
    /// This Dto will be used when a new user
    /// wants to register on the application.
    /// The user will share the following details for registration
    /// </summary>
    public class RegistrationRequestDto
    {
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? Role { get; set; }
    }
}
