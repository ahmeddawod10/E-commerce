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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Ecommerce.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CartRepository> _logger;
        private readonly TimeSpan _defaultExpiration;

        public CartRepository(
            IDistributedCache distributedCache,
            ILogger<CartRepository> logger,
            IConfiguration configuration)
        {
            _distributedCache = distributedCache;
            _logger = logger;

            // Get expiration from configuration or use default (30 days)
            var expirationDays = configuration.GetValue<int>("Cart:ExpirationDays", 30);
            _defaultExpiration = TimeSpan.FromDays(expirationDays);
        }

        private string GetCartKey(string userId) => $"cart:{userId}";

        public async Task<Cart?> GetAsync(string userId)
        {
            try
            {
                var cartKey = GetCartKey(userId);
                var cartJson = await _distributedCache.GetStringAsync(cartKey);

                if (string.IsNullOrEmpty(cartJson))
                {
                    _logger.LogDebug("Cart not found in Redis for user {UserId}", userId);
                    return null;
                }

                var cart = JsonSerializer.Deserialize<Cart>(cartJson, GetJsonSerializerOptions());
                _logger.LogDebug("Cart retrieved from Redis for user {UserId} with {ItemCount} items",
                    userId, cart?.Items.Count ?? 0);

                return cart;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing cart from Redis for user {UserId}", userId);
                // Return null instead of throwing to handle corrupted data gracefully
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart from Redis for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> SaveAsync(Cart cart)
        {
            try
            {
                var cartKey = GetCartKey(cart.UserId);
                var cartJson = JsonSerializer.Serialize(cart, GetJsonSerializerOptions());

                var options = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = _defaultExpiration
                };

                await _distributedCache.SetStringAsync(cartKey, cartJson, options);

                _logger.LogDebug("Cart saved to Redis for user {UserId} with {ItemCount} items",
                    cart.UserId, cart.Items.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving cart to Redis for user {UserId}", cart.UserId);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            try
            {
                var cartKey = GetCartKey(userId);
                await _distributedCache.RemoveAsync(cartKey);

                _logger.LogDebug("Cart deleted from Redis for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cart from Redis for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string userId)
        {
            try
            {
                var cartKey = GetCartKey(userId);
                var cartJson = await _distributedCache.GetStringAsync(cartKey);
                var exists = !string.IsNullOrEmpty(cartJson);

                _logger.LogDebug("Cart existence check in Redis for user {UserId}: {Exists}", userId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cart existence in Redis for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UpdateExpirationAsync(string userId, TimeSpan expiration)
        {
            try
            {
                // For Redis, we need to get the cart and save it again with new expiration
                var cart = await GetAsync(userId);
                if (cart == null)
                {
                    _logger.LogWarning("Cannot update expiration for non-existent cart for user {UserId}", userId);
                    return false;
                }

                var cartKey = GetCartKey(userId);
                var cartJson = JsonSerializer.Serialize(cart, GetJsonSerializerOptions());

                var options = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = expiration
                };

                await _distributedCache.SetStringAsync(cartKey, cartJson, options);

                _logger.LogDebug("Cart expiration updated in Redis for user {UserId} to {Expiration}",
                    userId, expiration);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart expiration in Redis for user {UserId}", userId);
                return false;
            }
        }

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

    }
}
