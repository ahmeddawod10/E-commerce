using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs;
using static Ecommerce.Application.DTOs.ResetDtos;

namespace Ecommerce.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
        Task<bool> SendOtpAsync(string email);
        Task<bool> VerifyOtpAsync(string email, string OtpCode);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);

    }

}
