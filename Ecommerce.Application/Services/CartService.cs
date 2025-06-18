// Ecommerce.Application.Services/CartService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Models; 
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<CartService> _logger;
        private readonly IMapper _mapper;

        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            ILogger<CartService> logger,
            IMapper mapper)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Result<CartDto>> GetCartAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("GetCartAsync was called with a null or empty userId.");
                    return Result<CartDto>.BadRequest("User ID cannot be null or empty.");
                }

                var cart = await _cartRepository.GetAsync(userId);

                if (cart == null)
                {
                    _logger.LogInformation("No existing cart found for user {UserId}. Returning an empty cart.", userId);
                    return Result<CartDto>.Ok(new CartDto { UserId = userId, TotalAmount = 0, TotalItems = 0, Items = new List<CartItemDtos>() }, "Empty cart retrieved successfully.");
                }

                cart.Items ??= new List<CartItem>();

                var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
                var products = productIds.Any() ? await _productRepository.GetProductsByIdsAsync(productIds) : new List<Product>();

                var cartItemsDto = new List<CartItemDtos>();
                foreach (var item in cart.Items)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        cartItemsDto.Add(new CartItemDtos
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Price = product.Price,
                            ImgUrl = product.ImageUrl,
                            Quantity = item.Quantity
                        });
                    }
                    else
                    {
                        _logger.LogWarning("Product with ID {ProductId} found in cart but not in product repository for user {UserId}.", item.ProductId, userId);
                    }
                }

                var cartDto = new CartDto
                {
                    UserId = userId,
                    TotalAmount = (int)(cart.TotalAmount ?? 0), 
                    TotalItems = cart.TotalItems ?? 0,
                    Items = cartItemsDto
                };

                return Result<CartDto>.Ok(cartDto, "Cart retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve cart for user {UserId}.", userId);
                return Result<CartDto>.BadRequest("Failed to retrieve cart due to an internal error.");
            }
        }

        public async Task<Result<CartDto>> AddToCartAsync(string userId, AddToCartRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("AddToCartAsync called with null or empty userId.");
                    return Result<CartDto>.BadRequest("User ID cannot be null or empty.");
                }

                if (request == null)
                {
                    _logger.LogWarning("AddToCartAsync called with null request for user {UserId}.", userId);
                    return Result<CartDto>.BadRequest("Request cannot be null.");
                }

                if (request.ProductId <= 0 || request.Quantity <= 0)
                {
                    _logger.LogWarning("AddToCartAsync called with invalid request data (ProductId: {ProductId}, Quantity: {Quantity}) for user {UserId}.", request.ProductId, request.Quantity, userId);
                    return Result<CartDto>.BadRequest("Invalid product ID or quantity. Product ID and quantity must be positive.");
                }

                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found when trying to add to cart for user {UserId}.", request.ProductId, userId);
                    return Result<CartDto>.NotFound($"Product with ID {request.ProductId} not found.");
                }

                var cart = await _cartRepository.GetAsync(userId)
                           ?? new Cart { UserId = userId, Items = new List<CartItem>() };

                cart.Items ??= new List<CartItem>();

                var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == request.ProductId);

                if (existingItem != null)
                {
                    if (product.Stock < (existingItem.Quantity + request.Quantity))
                    {
                        _logger.LogWarning("Insufficient stock for product {ProductId} (requested: {Requested}, current in cart: {Current}, available: {Available}) for user {UserId}.",
                                           request.ProductId, request.Quantity, existingItem.Quantity, product.Stock, userId);
                        return Result<CartDto>.BadRequest($"Insufficient stock for {product.Name}. Only {product.Stock - existingItem.Quantity} more units available.");
                    }
                    existingItem.Quantity += request.Quantity;
                    _logger.LogInformation("Updated existing item {ProductId} in cart for user {UserId}. New quantity: {Quantity}",
                        request.ProductId, userId, existingItem.Quantity);
                }
                else
                {
                    if (product.Stock < request.Quantity)
                    {
                        _logger.LogWarning("Insufficient stock for product {ProductId} (requested: {Requested}, available: {Available}) for user {UserId}.",
                                           request.ProductId, request.Quantity, product.Stock, userId);
                        return Result<CartDto>.BadRequest($"Insufficient stock for {product.Name}. Only {product.Stock} units available.");
                    }
                    var newItem = new CartItem
                    {
                        ProductId = request.ProductId,
                        Price = product.Price, 
                        Quantity = request.Quantity
                    };
                    cart.Items.Add(newItem);
                    _logger.LogInformation("Added new item {ProductId} to cart for user {UserId}. Quantity: {Quantity}",
                        request.ProductId, userId, request.Quantity);
                }


                var saveSuccess = await _cartRepository.SaveAsync(cart);
                if (!saveSuccess)
                {
                    _logger.LogError("Failed to save cart to repository for user {UserId} after adding item {ProductId}.", userId, request.ProductId);
                    return Result<CartDto>.BadRequest("Failed to save cart to repository.");
                }

                var updatedCartDto = new CartDto
                {
                    UserId = cart.UserId,
                    TotalAmount = (int)(cart.TotalAmount ?? 0),
                    TotalItems = cart.TotalItems ?? 0,
                    Items = new List<CartItemDtos>() 
                };
                return Result<CartDto>.Ok(updatedCartDto, "Product added successfully to cart.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item {ProductId} to cart for user {UserId}.", request?.ProductId, userId);
                return Result<CartDto>.BadRequest("Failed to add product to cart due to an internal error.");
            }
        }

        public async Task<Result<CartDto>> UpdateCartItemAsync(string userId, int productId, int quantity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("UpdateCartItemAsync called with null or empty userId.");
                    return Result<CartDto>.BadRequest("User ID cannot be null or empty.");
                }

                if (productId <= 0)
                {
                    _logger.LogWarning("UpdateCartItemAsync called with invalid productId ({ProductId}) for user {UserId}.", productId, userId);
                    return Result<CartDto>.BadRequest("Product ID must be positive.");
                }

                var cart = await _cartRepository.GetAsync(userId);
                if (cart == null || !cart.Items.Any())
                {
                    _logger.LogWarning("Cart not found or is empty for user {UserId} during item update.", userId);
                    return Result<CartDto>.NotFound($"Cart not found or is empty for user {userId}.");
                }

                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item == null)
                {
                    _logger.LogWarning("Product {ProductId} not found in cart for user {UserId} during update attempt.", productId, userId);
                    return Result<CartDto>.NotFound($"Product {productId} not found in cart for user {userId}.");
                }

                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    _logger.LogError("Product {ProductId} referenced in cart but not found in product catalog for user {UserId}.", productId, userId);
                    return Result<CartDto>.BadRequest($"Product {productId} is no longer available.");
                }

                if (quantity <= 0)
                {
                    cart.Items.Remove(item);
                    _logger.LogInformation("Removed item {ProductId} from cart for user {UserId} (quantity set to {Quantity}).",
                        productId, userId, quantity);
                }
                else
                {
                    if (quantity > item.Quantity && product.Stock < quantity)
                    {
                        _logger.LogWarning("Insufficient stock for product {ProductId} (requested: {Requested}, available: {Available}) for user {UserId}.",
                                           productId, quantity, product.Stock, userId);
                        return Result<CartDto>.BadRequest($"Insufficient stock for {product.Name}. Only {product.Stock} units available.");
                    }
                    item.Quantity = quantity;
                    // item.Price = product.Price; // Consider updating price if it can change
                    _logger.LogInformation("Updated item {ProductId} quantity to {Quantity} in cart for user {UserId}.",
                        productId, quantity, userId);
                }

                // No need for cart.RecalculateTotals() because TotalAmount and TotalItems are calculated properties

                var saveSuccess = await _cartRepository.SaveAsync(cart);
                if (!saveSuccess)
                {
                    _logger.LogError("Failed to save cart to repository for user {UserId} after updating item {ProductId}.", userId, productId);
                    return Result<CartDto>.BadRequest("Failed to save cart to repository.");
                }

                var cartDto = _mapper.Map<CartDto>(cart);
                return Result<CartDto>.Ok(cartDto, "Cart item updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {ProductId} for user {UserId}.", productId, userId);
                return Result<CartDto>.BadRequest("Failed to update cart item due to an internal error.");
            }
        }

        public async Task<Result<CartDto>> RemoveFromCartAsync(string userId, int productId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("RemoveFromCartAsync called with null or empty userId.");
                    return Result<CartDto>.BadRequest("User ID cannot be null or empty.");
                }

                if (productId <= 0)
                {
                    _logger.LogWarning("RemoveFromCartAsync called with invalid productId ({ProductId}) for user {UserId}.", productId, userId);
                    return Result<CartDto>.BadRequest("Product ID must be positive.");
                }

                var cart = await _cartRepository.GetAsync(userId);
                if (cart == null || !cart.Items.Any())
                {
                    _logger.LogWarning("Cart not found or is empty for user {UserId} during item removal.", userId);
                    return Result<CartDto>.NotFound($"Cart not found or is empty for user {userId}.");
                }

                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

                if (item != null)
                {
                    cart.Items.Remove(item);


                    var saveSuccess = await _cartRepository.SaveAsync(cart);
                    if (!saveSuccess)
                    {
                        _logger.LogError("Failed to save cart to repository for user {UserId} after removing item {ProductId}.", userId, productId);
                        return Result<CartDto>.BadRequest("Failed to save cart to repository.");
                    }

                    _logger.LogInformation("Removed item {ProductId} from cart for user {UserId}.", productId, userId);
                    var cartDto = _mapper.Map<CartDto>(cart);
                    return Result<CartDto>.Ok(cartDto, "Product removed successfully from cart.");
                }
                else
                {
                    _logger.LogWarning("Attempted to remove non-existent item {ProductId} from cart for user {UserId}.", productId, userId);
                    return Result<CartDto>.NotFound($"Product {productId} not found in cart for user {userId}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item {ProductId} from cart for user {UserId}.", productId, userId);
                return Result<CartDto>.BadRequest("Failed to remove item from cart due to an internal error.");
            }
        }

        public async Task<Result<bool>> ClearCartAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ClearCartAsync called with null or empty userId.");
                    return Result<bool>.BadRequest("User ID cannot be null or empty.");
                }

                var success = await _cartRepository.DeleteAsync(userId);

                if (success)
                {
                    _logger.LogInformation("Cleared cart for user {UserId}.", userId);
                    return Result<bool>.Ok(true, "Cart cleared successfully.");
                }
                else
                {
                    _logger.LogWarning("Failed to clear cart for user {UserId}. Cart might not exist or repository operation failed.", userId);
                    return Result<bool>.NotFound($"Cart not found for user {userId} or could not be cleared.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}.", userId);
                return Result<bool>.BadRequest("Failed to clear cart due to an internal error.");
            }
        }

        public async Task<Result<bool>> SetCartExpirationAsync(string userId, TimeSpan expiration)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("SetCartExpirationAsync called with null or empty userId.");
                    return Result<bool>.BadRequest("User ID cannot be null or empty.");
                }

                if (expiration <= TimeSpan.Zero)
                {
                    _logger.LogWarning("SetCartExpirationAsync called with non-positive expiration ({Expiration}) for user {UserId}.", expiration, userId);
                    return Result<bool>.BadRequest("Expiration must be a positive time span.");
                }

                var success = await _cartRepository.UpdateExpirationAsync(userId, expiration);

                if (success)
                {
                    _logger.LogInformation("Updated cart expiration for user {UserId} to {Expiration}.", userId, expiration);
                    return Result<bool>.Ok(true, "Cart expiration updated successfully.");
                }
                else
                {
                    _logger.LogWarning("Failed to set cart expiration for user {UserId}. Cart might not exist or update failed.", userId);
                    return Result<bool>.NotFound($"Cart not found for user {userId} or could not update expiration.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cart expiration for user {UserId}.", userId);
                return Result<bool>.BadRequest("Failed to set cart expiration due to an internal error.");
            }
        }

        private CartItem? FindExistingItem(Cart cart, int productId)
        {
            if (cart?.Items == null) return null;
            return cart.Items.FirstOrDefault(item => item.ProductId == productId);
        }
    }
}