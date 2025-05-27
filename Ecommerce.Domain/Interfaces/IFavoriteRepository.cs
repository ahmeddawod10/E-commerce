using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces
{
    public interface IFavoriteRepository
    {
        Task<Favorite?> GetAsync(string userId);
        Task<bool> SaveAsync(Favorite favorite);
        Task<bool> DeleteAsync(string userId);
        Task<bool> ExistsAsync(string userId);
        Task<bool> UpdateExpirationAsync(string userId, TimeSpan expiration);
    }
}
