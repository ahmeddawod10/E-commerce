using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Models;
using static Ecommerce.Application.DTOs.ResetDtos;

namespace Ecommerce.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterDto dto);
        Task<Result<LoginResponseDto>> LoginAsync(LoginDto model);
        Task<bool> SendOtpAsync(string email);
        Task<bool> VerifyOtpAsync(string email, string OtpCode);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
        Task<Result<TokenDto>> GenerateRefreshToken(string refreshToken);
        Task<Result<string>> Revoke();

    }

}
