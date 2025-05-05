using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs
{
    public class ResetDtos
    {
        public class ForgotPasswordDto
        {
            public string Email { get; set; }
        }

        public class VerifyOtpDto
        {
            public string Email { get; set; }
            public string OtpCode { get; set; }
        }

        public class ResetPasswordDto
        {
            public string Email { get; set; }
            public string NewPassword { get; set; }
            public string OtpCode { get; set; }
        }

    }
}
