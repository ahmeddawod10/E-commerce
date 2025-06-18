// Ecommerce.Infrastructure.Repositories/CartRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json; // For JsonSerializer
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed; // For IDistributedCache
using Microsoft.Extensions.Configuration; // For IConfiguration
using Microsoft.Extensions.Logging; // For ILogger
using StackExchange.Redis; // To specifically catch Redis-related exceptions

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
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (configuration != null)
            {
                var expirationDays = configuration.GetValue<int>("Cart:ExpirationDays", 30);
                _defaultExpiration = TimeSpan.FromDays(expirationDays);
            }
            else
            {
                _logger.LogWarning("IConfiguration was null in CartRepository constructor. Using default cart expiration of 30 days.");
                _defaultExpiration = TimeSpan.FromDays(30); 
            }
        }

        private string GetCartKey(string userId) => $"cart:{userId}";

        public async Task<Cart?> GetAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("GetAsync was called with a null or empty userId.");
                    return null;
                }

                var cartKey = GetCartKey(userId);
                var cartJson = await _distributedCache.GetStringAsync(cartKey);

                if (string.IsNullOrEmpty(cartJson))
                {
                    _logger.LogDebug("Cart not found in distributed cache for user {UserId}", userId);
                    return null;
                }

                var cart = JsonSerializer.Deserialize<Cart>(cartJson, GetJsonSerializerOptions());

                _logger.LogDebug("Cart retrieved from distributed cache for user {UserId} with {ItemCount} items.",
                    userId, cart?.Items?.Count ?? 0);

                return cart;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing cart from distributed cache for user {UserId}. Data might be corrupted.", userId);
                return null;
            }
            catch (RedisException ex) 
            {
                _logger.LogError(ex, "Redis connection error while retrieving cart for user {UserId}. Cache might be unavailable.", userId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving cart from distributed cache for user {UserId}.", userId);
                throw;
            }
        }

        public async Task<bool> SaveAsync(Cart cart)
        {
            try
            {
                if (cart == null)
                {
                    _logger.LogWarning("SaveAsync was called with a null cart object.");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(cart.UserId))
                {
                    _logger.LogWarning("SaveAsync was called with a cart object having a null or empty UserId.");
                    return false;
                }

                var cartKey = GetCartKey(cart.UserId);
                var cartJson = JsonSerializer.Serialize(cart, GetJsonSerializerOptions());

                var options = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = _defaultExpiration
                };

                await _distributedCache.SetStringAsync(cartKey, cartJson, options);

                _logger.LogDebug("Cart saved to distributed cache for user {UserId} with {ItemCount} items. Expiration set to {Expiration}.",
                    cart.UserId, cart.Items?.Count ?? 0, _defaultExpiration);

                return true;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error serializing cart for user {UserId} before saving to distributed cache.", cart?.UserId);
                return false;
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis connection error while saving cart for user {UserId}. Cache might be unavailable.", cart?.UserId);
                return false; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while saving cart to distributed cache for user {UserId}.", cart?.UserId);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("DeleteAsync was called with a null or empty userId.");
                    return false;
                }

                var cartKey = GetCartKey(userId);
                await _distributedCache.RemoveAsync(cartKey);

                _logger.LogDebug("Cart deleted from distributed cache for user {UserId}", userId);
                return true;
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis connection error while deleting cart for user {UserId}. Cache might be unavailable.", userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting cart from distributed cache for user {UserId}.", userId);
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ExistsAsync was called with a null or empty userId.");
                    return false;
                }

                var cartKey = GetCartKey(userId);
                var cartJson = await _distributedCache.GetStringAsync(cartKey); 

                var exists = !string.IsNullOrEmpty(cartJson);
                _logger.LogDebug("Cart existence check in distributed cache for user {UserId}: {Exists}", userId, exists);

                return exists;
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis connection error while checking cart existence for user {UserId}. Cache might be unavailable.", userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while checking cart existence in distributed cache for user {UserId}.", userId);
                return false;
            }
        }

        public async Task<bool> UpdateExpirationAsync(string userId, TimeSpan expiration)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("UpdateExpirationAsync was called with a null or empty userId.");
                    return false;
                }
                if (expiration <= TimeSpan.Zero)
                {
                    _logger.LogWarning("UpdateExpirationAsync was called with a non-positive expiration ({Expiration}) for user {UserId}.", expiration, userId);
                    return false;
                }

                var cart = await GetAsync(userId);
                if (cart == null)
                {
                    _logger.LogWarning("Cannot update expiration for non-existent cart for user {UserId}. GetAsync returned null.", userId);
                    return false;
                }

                var cartKey = GetCartKey(userId);
                var cartJson = JsonSerializer.Serialize(cart, GetJsonSerializerOptions());

                var options = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = expiration
                };

                await _distributedCache.SetStringAsync(cartKey, cartJson, options);

                _logger.LogDebug("Cart expiration updated in distributed cache for user {UserId} to {Expiration}.",
                    userId, expiration);

                return true;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error serializing cart for user {UserId} during expiration update to distributed cache.", userId);
                return false;
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis connection error while updating cart expiration for user {UserId}. Cache might be unavailable.", userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating cart expiration in distributed cache for user {UserId}.", userId);
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