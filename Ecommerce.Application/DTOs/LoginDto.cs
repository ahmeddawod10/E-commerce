using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs
{
    
        public class LoginDto
        {
            //public string userName {  get; set; }=string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

    public class LoginResponseDto
    {
        //public string userName {  get; set; }=string.Empty;
        public string Token { get; set; }
        public string UserId { get; set; }
        public  string UserName { get; set; }
        public  string Email { get; set; }
    }
}
