using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces
{
    public interface IFavoriteService
    {
        Task<Favorite> GetFavoritesAsync(string userId);
        Task<Favorite> AddFavoriteAsync(string userId, string productId, string productName);
        Task<Favorite> RemoveFavoriteAsync(string userId, string productId);
        Task<bool> ClearFavoritesAsync(string userId);

    }
}
