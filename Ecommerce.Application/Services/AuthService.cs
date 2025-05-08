using System.Threading.Tasks;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using static Ecommerce.Application.DTOs.ResetDtos;

namespace Ecommerce.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly RoleManager<IdentityRole> _roleManger;
        private readonly IMemoryCache _cache;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IEmailService emailService,
            RoleManager<IdentityRole> roleManger,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _roleManger = roleManger;
            _cache = cache;
        }

        public async Task<bool> RegisterAsync(RegisterDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.userName,
                Email = model.Email,
                FullName = model.fullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            var UserRole = await _userManager.AddToRoleAsync(user, "Customer");
            
            return result.Succeeded;
        
        
        
        }


        public async Task<bool> RegisterMerchantAsync(RegisterDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.userName,
                Email = model.Email,
                FullName = model.fullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            var UserRole = await _userManager.AddToRoleAsync(user, "Merchant");

            return result.Succeeded;



        }

        public async Task<string?> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return null;

            return await _tokenService.CreateTokenAsync(user);
        }

        public async Task<bool> SendOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var otp = new Random().Next(100000, 999999).ToString();
            _cache.Set(email, otp, TimeSpan.FromMinutes(5));

            var subject = "Your OTP Code";
            var body = $"Your OTP code is: {otp}";

            await _emailService.SendEmailAsync(email, subject, body);
            return true;
        }

        public Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var isValid = _cache.TryGetValue(email, out string cachedOtp) && cachedOtp == otp;
            return Task.FromResult(isValid);
        }


        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
             if (!_cache.TryGetValue(dto.Email, out string cachedOtp) || cachedOtp != dto.OtpCode)
                return false;

             var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return false;

             var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

             var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);

             _cache.Remove(dto.Email);

            return result.Succeeded;
        }

    }
}
