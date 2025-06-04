using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; // Not strictly needed by this version, but was in original
using System.Threading.Tasks;
// Removed AutoMapper and other unused using statements for clarity based on the provided code's actual usage.
// If AutoMapper is used elsewhere or by the entities/DTOs, you'll need to re-add it.
// using AutoMapper;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
// using Ecommerce.Application.Models; // Not used in the provided snippet
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
// using Microsoft.EntityFrameworkCore; // Not directly used in this service, likely in repository
// using Microsoft.Extensions.Caching.Distributed; // Not used in the provided snippet
using Microsoft.Extensions.Logging;
// using Newtonsoft.Json; // Not used in the provided snippet
// The following static using was specific to Cart.CartItem and Cart.ProductAttributes.
// If CartItem and ProductAttributes are directly accessible, it might not be needed,
// or if they are nested classes, it's fine.
using static Ecommerce.Domain.Entities.Cart;

namespace Ecommerce.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository; // Added for GetProductInfoAsync
        private readonly ILogger<CartService> _logger;
        // private readonly IMapper _mapper; // If you were using AutoMapper

        // Updated constructor to include IProductRepository
        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository, // Added
            ILogger<CartService> logger)
        // IMapper mapper) // Add if using AutoMapper
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository)); // Added
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); // Add if using AutoMapper
        }

        public async Task<Cart> GetCartAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("GetCartAsync called with null or empty userId.");
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                var cart = await _cartRepository.GetAsync(userId);

                if (cart == null)
                {
                    _logger.LogInformation("Creating new cart for user {UserId}", userId);
                    cart = new Cart { UserId = userId, Items = new List<CartItem>() }; // Ensure Items is initialized
                }
                else if (cart.Items == null) // Defensive check
                {
                    cart.Items = new List<CartItem>();
                }


                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user {UserId}", userId);
                throw; // Re-throw to allow higher-level error handling
            }
        }

        public async Task<Cart> AddToCartAsync(string userId, AddToCartRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("AddToCartAsync called with null or empty userId.");
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                if (request == null)
                {
                    _logger.LogWarning("AddToCartAsync called with null request for user {UserId}.", userId);
                    throw new ArgumentNullException(nameof(request));
                }

                // Validate request properties (basic example)
                if (request.ProductId==null || request.Quantity <= 0 || request.Price < 0)
                {
                    _logger.LogWarning("AddToCartAsync called with invalid request data for user {UserId}.", userId);
                    throw new ArgumentException("Invalid request data: ProductId, Quantity, or Price is invalid.");
                }

                var cart = await GetCartAsync(userId); // This ensures cart.Items is initialized

                var existingItem = FindExistingItem(cart, request.ProductId, request.ProductAttributes);

                if (existingItem != null)
                {
                    existingItem.Quantity += request.Quantity;
                    //existingItem.Price = request.Price;
                    //existingItem.ProductName = request.ProductName;
                   // existingItem.ImageUrl = request.ImageUrl;

                    _logger.LogInformation("Updated existing item {ProductId} in cart for user {UserId}. New quantity: {Quantity}",
                        request.ProductId, userId, existingItem.Quantity);
                }
                else
                {
                    var newItem = new CartItem // Assuming CartItem is directly usable
                    {
                        ProductId = request.ProductId,
                        //ProductName = request.ProductName,
                       // Price = request.Price,
                        Quantity = request.Quantity,
                        //ImageUrl = request.ImageUrl,
                        //ProductAttributes = request.ProductAttributes?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                    };
                    cart.Items.Add(newItem);

                    _logger.LogInformation("Added new item {ProductId} to cart for user {UserId}. Quantity: {Quantity}",
                        request.ProductId, userId, request.Quantity);
                }

                cart.LastModified = DateTime.UtcNow;

                var saveSuccess = await _cartRepository.SaveAsync(cart);
                if (!saveSuccess)
                {
                    _logger.LogError("Failed to save cart to repository for user {UserId} after adding item {ProductId}.", userId, request.ProductId);
                    throw new InvalidOperationException("Failed to save cart to repository.");
                }

                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item {ProductId} to cart for user {UserId}",
                    request?.ProductId, userId); // request?.ProductId handles case where request itself is null
                throw;
            }
        }

        public async Task<Cart> UpdateCartItemAsync(string userId, int productId, int quantity)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UpdateCartItemAsync called with null or empty userId.");
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                if(productId == null)
                {
                    _logger.LogWarning("UpdateCartItemAsync called with null or empty productId for user {UserId}.", userId);
                    throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));
                }

                var cart = await GetCartAsync(userId);
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

                if (item == null)
                {
                    _logger.LogWarning("Product {ProductId} not found in cart for user {UserId} during update attempt.", productId, userId);
                    // Consider if throwing an exception or returning a specific status is more appropriate
                    throw new InvalidOperationException($"Product {productId} not found in cart for user {userId}.");
                }

                if (quantity <= 0)
                {
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
                    _logger.LogError("Failed to save cart to repository for user {UserId} after updating item {ProductId}.", userId, productId);
                    throw new InvalidOperationException("Failed to save cart to repository.");
                }

                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {ProductId} for user {UserId}", productId, userId);
                throw;
            }
        }

        public async Task<Cart> RemoveFromCartAsync(string userId, int productId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("RemoveFromCartAsync called with null or empty userId.");
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                //if (string.IsNullOrEmpty(productId))
                if(productId==null)
                {
                    _logger.LogWarning("RemoveFromCartAsync called with null or empty productId for user {UserId}.", userId);
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
                        _logger.LogError("Failed to save cart to repository for user {UserId} after removing item {ProductId}.", userId, productId);
                        throw new InvalidOperationException("Failed to save cart to repository.");
                    }

                    _logger.LogInformation("Removed item {ProductId} from cart for user {UserId}", productId, userId);
                }
                else
                {
                    _logger.LogWarning("Attempted to remove non-existent item {ProductId} from cart for user {UserId}",
                        productId, userId);
                    // No cart modification needed if item not found, so just return current cart.
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
                    _logger.LogWarning("ClearCartAsync called with null or empty userId.");
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                var success = await _cartRepository.DeleteAsync(userId);

                if (success)
                {
                    _logger.LogInformation("Cleared cart for user {UserId}", userId);
                }
                else
                {
                    // This might indicate the cart didn't exist or an issue with deletion.
                    _logger.LogWarning("Failed to clear cart for user {UserId}. Repository reported no success.", userId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                // Depending on policy, you might want to re-throw or return false.
                // Returning false aligns with the method signature expecting a bool.
                return false;
            }
        }

        public async Task<bool> CartExistsAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogInformation("CartExistsAsync called with null or empty userId. Returning false.");
                    return false; // An empty/null userId cannot have an existing cart.
                }

                return await _cartRepository.ExistsAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cart existence for user {UserId}", userId);
                return false; // In case of error, assume cart does not exist or cannot be verified.
            }
        }

        public async Task<Cart> MergeCartsAsync(string sourceUserId, string targetUserId)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceUserId))
                {
                    _logger.LogWarning("MergeCartsAsync called with null or empty sourceUserId.");
                    throw new ArgumentException("Source User ID must be provided.", nameof(sourceUserId));
                }
                if (string.IsNullOrEmpty(targetUserId))
                {
                    _logger.LogWarning("MergeCartsAsync called with null or empty targetUserId.");
                    throw new ArgumentException("Target User ID must be provided.", nameof(targetUserId));
                }
                if (sourceUserId == targetUserId)
                {
                    _logger.LogInformation("MergeCartsAsync called with same source and target user ID ({UserId}). No action needed.", sourceUserId);
                    return await GetCartAsync(targetUserId); // Return the cart as is.
                }

                var sourceCart = await GetCartAsync(sourceUserId);
                var targetCart = await GetCartAsync(targetUserId); // Ensures targetCart.Items is initialized

                if (!sourceCart.Items.Any())
                {
                    _logger.LogInformation("Source cart for user {SourceUserId} is empty. No items to merge into cart for {TargetUserId}.", sourceUserId, targetUserId);
                    // Optionally delete empty source cart
                    await _cartRepository.DeleteAsync(sourceUserId);
                    return targetCart;
                }

                foreach (var sourceItem in sourceCart.Items)
                {
                    var existingItem = FindExistingItem(targetCart, sourceItem.ProductId, sourceItem.ProductAttributes);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += sourceItem.Quantity;
                        // Policy for conflicting data: use source, target, or newest. Here, using source for simplicity.
                        existingItem.Price = sourceItem.Price;
                        existingItem.ProductName = sourceItem.ProductName;
                        existingItem.ImageUrl = sourceItem.ImageUrl;
                    }
                    else
                    {
                        // Create a new CartItem for the target cart
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

                var saveSuccess = await _cartRepository.SaveAsync(targetCart);
                if (saveSuccess)
                {
                    // Only delete source cart if merge and save were successful
                    await _cartRepository.DeleteAsync(sourceUserId);
                    _logger.LogInformation("Successfully merged cart from {SourceUserId} to {TargetUserId} and deleted source cart.",
                        sourceUserId, targetUserId);
                }
                else
                {
                    _logger.LogError("Failed to save merged cart for target user {TargetUserId} after merging from {SourceUserId}. Source cart was not deleted.", targetUserId, sourceUserId);
                    throw new InvalidOperationException("Failed to save merged cart.");
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
                    _logger.LogWarning("SetCartExpirationAsync called with null or empty userId.");
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                if (expiration <= TimeSpan.Zero)
                {
                    _logger.LogWarning("SetCartExpirationAsync called with non-positive expiration for user {UserId}.", userId);
                    throw new ArgumentOutOfRangeException(nameof(expiration), "Expiration must be a positive time span.");
                }

                var success = await _cartRepository.UpdateExpirationAsync(userId, expiration);

                if (success)
                {
                    _logger.LogInformation("Updated cart expiration for user {UserId} to {Expiration}",
                        userId, expiration);
                }
                else
                {
                    _logger.LogWarning("Failed to set cart expiration for user {UserId}. Repository reported no success (cart might not exist or update failed).", userId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cart expiration for user {UserId}", userId);
                return false;
            }
        }

        // New method to get product information
        public async Task<ProductInfoDto> GetProductInfoAsync(string productId, int quantity)
        {
            try
            {
                if (string.IsNullOrEmpty(productId))
                {
                    _logger.LogWarning("GetProductInfoAsync called with null or empty productId.");
                    throw new ArgumentException("Product ID cannot be null or empty.", nameof(productId));
                }

                if (quantity <= 0)
                {
                    _logger.LogWarning("GetProductInfoAsync called with non-positive quantity for productId {ProductId}.", productId);
                    throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
                }

                // Fetch product details from the product repository
                // Assuming Product entity has Id, Name, Price properties
                var product = await _productRepository.GetByIdAsync(int.Parse(productId));

                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found in repository.", productId);
                    throw new KeyNotFoundException($"Product with ID {productId} not found."); // More specific exception
                }


                // Create and return the DTO
                var productInfo = new ProductInfoDto
                {
                    ProductId = product.Id.ToString(),
                    Name = product.Name,
                    Price = product.Price, // Assuming product.Price is decimal
                    Quantity = quantity,
                    TotalPrice = product.Price * quantity
                    // You can add other properties from 'product' to 'ProductInfoDto' if needed
                    // e.g., Description = product.Description, ImageUrl = product.ImageUrl
                };

                _logger.LogInformation("Successfully retrieved info for product {ProductId} with quantity {Quantity}.", productId, quantity);
                return productInfo;
            }
            catch (KeyNotFoundException knfex) // Catch specific exception first
            {
                _logger.LogError(knfex, "Product not found error in GetProductInfoAsync for product {ProductId}.", productId);
                throw; // Re-throw to allow specific handling by caller
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product info for product {ProductId} with quantity {Quantity}.", productId, quantity);
                throw; // Re-throw to allow higher-level error handling
            }
        }


        private CartItem? FindExistingItem(Cart cart, int productId, Dictionary<string, string>? attributes)
        {
            if (cart?.Items == null) return null; // Defensive check

            return cart.Items.FirstOrDefault(item =>
                item.ProductId == productId &&
                AreAttributesEqual(item.ProductAttributes, attributes));
        }

        private bool AreAttributesEqual(Dictionary<string, string>? attr1, Dictionary<string, string>? attr2)
        {
            // Both null or empty are considered equal
            bool attr1IsNullOrEmpty = attr1 == null || !attr1.Any();
            bool attr2IsNullOrEmpty = attr2 == null || !attr2.Any();

            if (attr1IsNullOrEmpty && attr2IsNullOrEmpty) return true;
            if (attr1IsNullOrEmpty || attr2IsNullOrEmpty) return false; // One is empty/null, the other is not

            // At this point, both attr1 and attr2 are non-null and non-empty
            if (attr1.Count != attr2.Count) return false;

            // Order-independent comparison
            return attr1.All(kvp1 =>
                attr2.TryGetValue(kvp1.Key, out var value2) &&
                string.Equals(kvp1.Value, value2, StringComparison.Ordinal)); // Use StringComparison for robustness if needed
        }
    }
}