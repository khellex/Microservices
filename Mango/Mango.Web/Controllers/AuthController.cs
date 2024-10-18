﻿using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<IActionResult> Login()
        {
            LoginRequestDto loginRequestDto = new();
            return View();
        }
        public async Task<IActionResult> Register()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            return View();
        }
    }
}
