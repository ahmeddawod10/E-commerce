using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;

namespace Ecommerce.Infrastructure.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<FavoriteRepository> _logger;
        private readonly TimeSpan _defaultExpiration;

        public FavoriteRepository(
            IDistributedCache distributedCache,
            ILogger<FavoriteRepository> logger,
            IConfiguration configuration)
        {
            _distributedCache = distributedCache;
            _logger = logger;

            var expirationDays = configuration.GetValue<int>("Favorite:ExpirationDays", 30);
            _defaultExpiration = TimeSpan.FromDays(expirationDays);
        }

        private string GetFavoriteKey(string userId) => $"favorite:{userId}";

        public async Task<Favorite?> GetAsync(string userId)
        {
            try
            {
                var key = GetFavoriteKey(userId);
                var json = await _distributedCache.GetStringAsync(key);

                if (string.IsNullOrEmpty(json))
                    return null;

                return JsonSerializer.Deserialize<Favorite>(json, GetOptions());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving favorites for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> SaveAsync(Favorite favorite)
        {
            try
            {
                var key = GetFavoriteKey(favorite.UserId);
                var json = JsonSerializer.Serialize(favorite, GetOptions());

                await _distributedCache.SetStringAsync(key, json, new DistributedCacheEntryOptions
                {
                    SlidingExpiration = _defaultExpiration
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving favorites for user {UserId}", favorite.UserId);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            try
            {
                var key = GetFavoriteKey(userId);
                await _distributedCache.RemoveAsync(key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting favorites for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string userId)
        {
            var key = GetFavoriteKey(userId);
            var json = await _distributedCache.GetStringAsync(key);
            return !string.IsNullOrEmpty(json);
        }

        public async Task<bool> UpdateExpirationAsync(string userId, TimeSpan expiration)
        {
            var favorite = await GetAsync(userId);
            if (favorite == null) return false;

            var key = GetFavoriteKey(userId);
            var json = JsonSerializer.Serialize(favorite, GetOptions());

            await _distributedCache.SetStringAsync(key, json, new DistributedCacheEntryOptions
            {
                SlidingExpiration = expiration
            });

            return true;
        }

        private static JsonSerializerOptions GetOptions() => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }


}


