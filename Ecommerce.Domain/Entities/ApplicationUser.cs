using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Role {  get; set; }
        public string? OtpCode { get; set; }
        public DateTime? OtpExpiryTime { get; set; }
        public bool? IsActive { get; set; } = true;
        
        [MaxLength(200)]
        public string? RefreshToken { get; set; } = string.Empty;

         
        public DateTime? RefreshExpiredAt { get; set; }
    }
}
