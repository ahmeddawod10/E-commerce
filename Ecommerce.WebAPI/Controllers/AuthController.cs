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
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNet.Identity;


namespace Ecommerce.WebAPI.Controllers
{ 

    namespace WebAPI.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class AuthController : BaseApiController
        {
            private readonly IAuthService _authService;
 
            public AuthController(IAuthService authService)
            {
                _authService = authService;
            }

            [HttpPost("register")]
            public async Task<IActionResult> Register([FromBody] RegisterDto model)
            {
                var result = await _authService.RegisterAsync(model);
                if (!result)
                    return BadRequest("Registration failed");

                return Ok("Registered successfully.");
            }

            [HttpPost("register_Admin")]
            public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDto model)
            {
                var result = await _authService.RegisterAdminAsync(model);
                if (!result)
                    return BadRequest("Registration failed");

                return Ok("Registered successfully.");
            }


            [HttpPost("register_Merchant")]
            public async Task<IActionResult> RegisterMerchant([FromBody] RegisterDto model)
            {
                var result = await _authService.RegisterMerchantAsync(model);
                if (!result)
                    return BadRequest("Registration failed");

                return Ok("Registered successfully.");
            }

            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginDto model)
            {
                var token = await _authService.LoginAsync(model);

                return CreatedResponse(token);
            }

            [HttpPost("send-otp")]
            public async Task<IActionResult> SendOtp([FromBody] string email)
            {
                var result = await _authService.SendOtpAsync(email);
                if (!result)
                    return BadRequest("Failed to send OTP.");
                return Ok("OTP sent successfully.");
            }

            [HttpPost("verify-otp")]
            public async Task<IActionResult> VerifyOtp([FromBody] ResetDtos.VerifyOtpDto dto)
            {
                var result = await _authService.VerifyOtpAsync(dto.Email, dto.OtpCode);
                if (!result)
                    return BadRequest("Invalid or expired OTP.");
                return Ok("OTP verified.");
            }

            [HttpPost("reset-password")]
            [Authorize(Roles = "Customer")]
            public async Task<IActionResult> ResetPassword([FromBody] ResetDtos.ResetPasswordDto dto)
            {
                var result = await _authService.ResetPasswordAsync(dto);
                if (!result)
                    return BadRequest("Password reset failed.");
                return Ok("Password reset successful.");
            }



            [HttpGet("send-test-email")]
            public async Task<IActionResult> SendTestEmail([FromServices] IEmailService emailService)
            {
                try
                {
                    await emailService.SendEmailAsync("recipient-email@example.com", "Test Email", "This is a test.");
                    return Ok("Email sent successfully.");
                }
                catch (Exception ex)
                {
                    return BadRequest($"Failed to send email: {ex.Message}");
                }
            }


            [HttpPost("refresh-token")]
            [AllowAnonymous]
            public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
            {
                var result = await _authService.GenerateRefreshToken(refreshToken);
                if (!result.Success)
                    return Unauthorized(result.Message);

                return Ok(result.Data);  
            }

            [HttpPost("revoke-token")]
            [Authorize(Roles = "Customer")]
            public async Task<IActionResult> RevokeToken()
            {
                var result = await _authService.Revoke();
                if (!result.Success)
                    return BadRequest(result.Message);

                return Ok(result.Data);  
            }


        }
    }


}
