using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Application.DTOs;
using Ecommerce.WebAPI.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
namespace Ecommerce.WebAPI.Controllers
{ 

    namespace WebAPI.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class AuthController : ControllerBase
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly SignInManager<ApplicationUser> _signInManager;
            private readonly ITokenService _tokenService;

            public AuthController(
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                ITokenService tokenService)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _tokenService = tokenService;
            }

            [HttpPost("register")]
            public async Task<IActionResult> Register([FromBody] RegisterDto model)
            {
                var user = new ApplicationUser
                {
                    UserName = model.userName,
                    Email = model.Email,
                    FullName = model.fullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return Ok("Registered successfully.");
            }

            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginDto model)
            {
                var user = await _userManager.FindByNameAsync(model.Email);

                if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                    return Unauthorized("Invalid credentials");

                var token = await _tokenService.CreateTokenAsync(user);
                return Ok(new { token });
            }
        }


    }


}
