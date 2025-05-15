using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Models;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        private readonly IHttpContextAccessor _httpContextAccessor;


        public AuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IEmailService emailService,
            RoleManager<IdentityRole> roleManger,
            IMemoryCache cache,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _roleManger = roleManger;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
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




        public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto model)
        {


            var user = await _userManager.FindByEmailAsync(model.Email);
            if ((bool)!user.IsActive)
            {
                return Result<LoginResponseDto>.Forbidden("user is blocked");


            }

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Result<LoginResponseDto>.Unauthorized("invalid credentials");



            var token = await _tokenService.CreateTokenAsync(user);
            if (token == null)
            {
                return Result<LoginResponseDto>.Internal("internal server error");

            }

            var refreshToken =  _tokenService.GenerateRefreshToken();
            if (refreshToken == null)
            {
                return Result<LoginResponseDto>.Internal("internal server error");

            }
            user.RefreshToken = refreshToken;
            user.RefreshExpiredAt = DateTime.Now.AddMinutes(5);

            await _userManager.UpdateAsync(user);
            var tokenDto = new TokenDto
            {
                Token = token,
                RefreshToken = refreshToken
            };
            var userDto = new LoginResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                TokenDto=tokenDto

            };
            return Result<LoginResponseDto>.Ok(userDto, "login successfully");


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

        public async Task<Result<TokenDto>> GenerateRefreshToken(string refreshToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
            if (user == null) 
            {
                return Result<TokenDto>.Unauthorized("user not authorize");
            }
            var time= user.RefreshExpiredAt>DateTime.Now.ToLocalTime();
            if (!time) { return Result<TokenDto>.Unauthorized("expire time"); }
            var accessToken = await _tokenService.CreateTokenAsync(user);

            var refresh_Token =  _tokenService.GenerateRefreshToken();
            user.RefreshToken= refresh_Token;

            await _userManager.UpdateAsync(user);
            var tokenDto = new TokenDto
            {
                Token = accessToken,
                RefreshToken = refresh_Token
            };
            return Result<TokenDto>.Ok(tokenDto, "Refresh Token updated");

        }

        public async Task<Result<string>> Revoke()
        {
            var context = _httpContextAccessor.HttpContext;
            var userId = CurrentUserID.GetUserId(context);
            if(userId == null)
            {
                return Result<string>.NotFound("user not found");
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(x=>x.Id == userId);
            if (user == null)
            {
                return Result<string>.NotFound("user not found");

            }


            user.RefreshToken = null;
            var updated=await _userManager.UpdateAsync(user);
            if(!updated.Succeeded)
            {
                return Result<string>.Internal("error");
            }
            return Result<string>.Ok("");

        }


    }
}
