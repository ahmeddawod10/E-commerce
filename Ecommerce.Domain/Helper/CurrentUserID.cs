using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Domain.Helper
{
    public static class CurrentUserID
    {
        public static string? GetUserId(HttpContext httpContext)
        {
            return GetClaim(httpContext, "sub") ?? GetClaim(httpContext, ClaimTypes.NameIdentifier);
        }
        public static string? GetClaim(HttpContext httpContext, string claimType)
        {
            return httpContext?.User?.FindFirst(claimType)?.Value;
        }
    }
}
