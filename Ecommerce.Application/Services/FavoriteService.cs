using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Models;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;

        public FavoriteService(IFavoriteRepository favoriteRepository)
        {
            _favoriteRepository = favoriteRepository;
        }

        public async Task<Favorite> GetFavoritesAsync(string userId)
        {
            return await _favoriteRepository.GetAsync(userId) ?? new Favorite { UserId = userId };
        }

        public async Task<Favorite> AddFavoriteAsync(string userId, string productId)
        {
            var favorite = await GetFavoritesAsync(userId);

            if (!favorite.Items.Any(i => i.ProductId == productId))
            {
                favorite.Items.Add(new FavoriteItem
                {
                    ProductId = productId,
                });

                favorite.UpdatedAt = DateTime.UtcNow;
                await _favoriteRepository.SaveAsync(favorite);
            }

            return favorite;
        }

        public async Task<Favorite> RemoveFavoriteAsync(string userId, string productId)
        {
            var favorite = await GetFavoritesAsync(userId);
            favorite.Items.RemoveAll(i => i.ProductId == productId);
            favorite.UpdatedAt = DateTime.UtcNow;

            await _favoriteRepository.SaveAsync(favorite);
            return favorite;
        }

        public async Task<bool> ClearFavoritesAsync(string userId)
        {
            return await _favoriteRepository.DeleteAsync(userId);
        }
    }


}

