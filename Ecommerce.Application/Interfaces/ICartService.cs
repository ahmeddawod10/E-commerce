using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Models;
using Ecommerce.Domain.Entities;
using static Ecommerce.Domain.Entities.Cart;

namespace Ecommerce.Application.Interfaces
{
    public interface ICartService
    {
        Task<Result<CartDto>> GetCartAsync(string userId);
        Task<Result<CartDto>> AddToCartAsync(string userId, AddToCartRequest request);
        Task<Result<CartDto>> UpdateCartItemAsync(string userId, int productId, int quantity);
        Task<Result<CartDto>> RemoveFromCartAsync(string userId, int productId);
        Task<Result<bool>> ClearCartAsync(string userId);
        Task<Result<bool>> SetCartExpirationAsync(string userId, TimeSpan expiration);
    }
}
