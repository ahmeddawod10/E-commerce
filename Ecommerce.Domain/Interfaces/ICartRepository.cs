using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces
{
    public interface ICartRepository
    {
        //Task<Cart?> GetCartAsync(string userId);
        //Task SaveCartAsync(Cart cart);
        //Task RemoveCartAsync(string userId);
        Task<Cart?> GetAsync(string userId);
        Task<bool> SaveAsync(Cart cart);
        Task<bool> DeleteAsync(string userId);
        Task<bool> ExistsAsync(string userId);
        Task<bool> UpdateExpirationAsync(string userId, TimeSpan expiration);
    }
}
