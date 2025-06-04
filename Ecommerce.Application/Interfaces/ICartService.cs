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
        Task<Cart> GetCartAsync(string userId);
        Task<Cart> AddToCartAsync(string userId, AddToCartRequest request);
        Task<Cart> UpdateCartItemAsync(string userId, int productId, int quantity);
        Task<Cart> RemoveFromCartAsync(string userId, int productId);
        Task<bool> ClearCartAsync(string userId);
        Task<bool> CartExistsAsync(string userId);
        //Task<Cart> MergeCartsAsync(string sourceUserId, string targetUserId);
        Task<bool> SetCartExpirationAsync(string userId, TimeSpan expiration);
    }
}
