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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static Ecommerce.Domain.Entities.Cart;

namespace Ecommerce.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository cartRepository, ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task<Cart> GetCartAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                var cart = await _cartRepository.GetAsync(userId);

                if (cart == null)
                {
                    _logger.LogInformation("Creating new cart for user {UserId}", userId);
                    cart = new Cart { UserId = userId };
                }

                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Cart> AddToCartAsync(string userId, AddToCartRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                var cart = await GetCartAsync(userId);

                // Check if item already exists in cart (same product and attributes)
                var existingItem = FindExistingItem(cart, request.ProductId, request.ProductAttributes);

                if (existingItem != null)
                {
                    // Update existing item
                    existingItem.Quantity += request.Quantity;
                    existingItem.Price = request.Price; // Update price in case it changed
                    existingItem.ProductName = request.ProductName; // Update name in case it changed
                    existingItem.ImageUrl = request.ImageUrl; // Update image in case it changed

                    _logger.LogInformation("Updated existing item {ProductId} in cart for user {UserId}. New quantity: {Quantity}",
                        request.ProductId, userId, existingItem.Quantity);
                }
                else
                {
                    // Add new item to cart
                    var newItem = new CartItem
                    {
                        ProductId = request.ProductId,
                        ProductName = request.ProductName,
                        Price = request.Price,
                        Quantity = request.Quantity,
                        ImageUrl = request.ImageUrl,
                        ProductAttributes = request.ProductAttributes?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                    };

                    cart.Items.Add(newItem);

                    _logger.LogInformation("Added new item {ProductId} to cart for user {UserId}. Quantity: {Quantity}",
                        request.ProductId, userId, request.Quantity);
                }

                cart.LastModified = DateTime.UtcNow;

                var saveSuccess = await _cartRepository.SaveAsync(cart);
                if (!saveSuccess)
                {
                    throw new InvalidOperationException("Failed to save cart to repository");
                }

                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item {ProductId} to cart for user {UserId}",
                    request?.ProductId, userId);
                throw;
            }
        }

        public async Task<Cart> UpdateCartItemAsync(string userId, string productId, int quantity)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                if (string.IsNullOrEmpty(productId))
                {
                    throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));
                }

                var cart = await GetCartAsync(userId);
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

                if (item == null)
                {
                    throw new InvalidOperationException($"Product {productId} not found in cart for user {userId}");
                }

                if (quantity <= 0)
                {
                    // Remove item if quantity is 0 or negative
                    cart.Items.Remove(item);
                    _logger.LogInformation("Removed item {ProductId} from cart for user {UserId} (quantity set to {Quantity})",
                        productId, userId, quantity);
                }
                else
                {
                    item.Quantity = quantity;
                    _logger.LogInformation("Updated item {ProductId} quantity to {Quantity} in cart for user {UserId}",
                        productId, quantity, userId);
                }

                cart.LastModified = DateTime.UtcNow;

                var saveSuccess = await _cartRepository.SaveAsync(cart);
                if (!saveSuccess)
                {
                    throw new InvalidOperationException("Failed to save cart to repository");
                }

                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {ProductId} for user {UserId}", productId, userId);
                throw;
            }
        }

        public async Task<Cart> RemoveFromCartAsync(string userId, string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                if (string.IsNullOrEmpty(productId))
                {
                    throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));
                }

                var cart = await GetCartAsync(userId);
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

                if (item != null)
                {
                    cart.Items.Remove(item);
                    cart.LastModified = DateTime.UtcNow;

                    var saveSuccess = await _cartRepository.SaveAsync(cart);
                    if (!saveSuccess)
                    {
                        throw new InvalidOperationException("Failed to save cart to repository");
                    }

                    _logger.LogInformation("Removed item {ProductId} from cart for user {UserId}", productId, userId);
                }
                else
                {
                    _logger.LogWarning("Attempted to remove non-existent item {ProductId} from cart for user {UserId}",
                        productId, userId);
                }

                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item {ProductId} from cart for user {UserId}", productId, userId);
                throw;
            }
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                var success = await _cartRepository.DeleteAsync(userId);

                if (success)
                {
                    _logger.LogInformation("Cleared cart for user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to clear cart for user {UserId}", userId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> CartExistsAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                return await _cartRepository.ExistsAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cart existence for user {UserId}", userId);
                return false;
            }
        }

        public async Task<Cart> MergeCartsAsync(string sourceUserId, string targetUserId)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceUserId) || string.IsNullOrEmpty(targetUserId))
                {
                    throw new ArgumentException("Both source and target user IDs must be provided");
                }

                var sourceCart = await GetCartAsync(sourceUserId);
                var targetCart = await GetCartAsync(targetUserId);

                // Merge items from source cart to target cart
                foreach (var sourceItem in sourceCart.Items)
                {
                    var existingItem = FindExistingItem(targetCart, sourceItem.ProductId, sourceItem.ProductAttributes);

                    if (existingItem != null)
                    {
                        // Merge quantities and use the most recent price
                        existingItem.Quantity += sourceItem.Quantity;
                        existingItem.Price = sourceItem.Price;
                        existingItem.ProductName = sourceItem.ProductName;
                        existingItem.ImageUrl = sourceItem.ImageUrl;
                    }
                    else
                    {
                        // Add new item to target cart
                        targetCart.Items.Add(new CartItem
                        {
                            ProductId = sourceItem.ProductId,
                            ProductName = sourceItem.ProductName,
                            Price = sourceItem.Price,
                            Quantity = sourceItem.Quantity,
                            ImageUrl = sourceItem.ImageUrl,
                            ProductAttributes = sourceItem.ProductAttributes?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                        });
                    }
                }

                targetCart.LastModified = DateTime.UtcNow;

                // Save merged cart and delete source cart
                var saveSuccess = await _cartRepository.SaveAsync(targetCart);
                if (saveSuccess)
                {
                    await _cartRepository.DeleteAsync(sourceUserId);
                    _logger.LogInformation("Successfully merged cart from {SourceUserId} to {TargetUserId}",
                        sourceUserId, targetUserId);
                }

                return targetCart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging carts from {SourceUserId} to {TargetUserId}",
                    sourceUserId, targetUserId);
                throw;
            }
        }

        public async Task<bool> SetCartExpirationAsync(string userId, TimeSpan expiration)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                var success = await _cartRepository.UpdateExpirationAsync(userId, expiration);

                if (success)
                {
                    _logger.LogInformation("Updated cart expiration for user {UserId} to {Expiration}",
                        userId, expiration);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cart expiration for user {UserId}", userId);
                return false;
            }
        }

        private CartItem? FindExistingItem(Cart cart, string productId, Dictionary<string, string>? attributes)
        {
            return cart.Items.FirstOrDefault(item =>
                item.ProductId == productId &&
                AreAttributesEqual(item.ProductAttributes, attributes));
        }

        private bool AreAttributesEqual(Dictionary<string, string>? attr1, Dictionary<string, string>? attr2)
        {
            if (attr1 == null && attr2 == null) return true;
            if (attr1 == null || attr2 == null) return false;
            if (attr1.Count != attr2.Count) return false;

            return attr1.All(kvp => attr2.ContainsKey(kvp.Key) && attr2[kvp.Key] == kvp.Value);
        }
    }
}




