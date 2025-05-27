using Ecommerce.Application.Interfaces;
using System.Security.Claims;
using Ecommerce.Application.Models;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Domain.Entities.Cart;

namespace Ecommerce.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : BaseApiController
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        private CartResponse MapToCartResponse(Cart cart)
        {
            return new CartResponse
            {
                UserId = cart.UserId,
                Items = cart.Items,
                TotalAmount = cart.TotalAmount,
                TotalItems = cart.TotalItems,
                CreatedAt = cart.LastModified
            };
        }

        [HttpGet]
        public async Task<ActionResult<CartResponse>> GetCart()
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.GetCartAsync(userId);
                return Ok(MapToCartResponse(cart));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart");
                return StatusCode(500, new { message = "An error occurred while retrieving the cart" });
            }
        }

        [HttpPost("items")]
        public async Task<ActionResult<CartResponse>> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetUserId();
                var cart = await _cartService.AddToCartAsync(userId, request);
                return Ok(MapToCartResponse(cart));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, new { message = "An error occurred while adding the item to cart" });
            }
        }

        [HttpPut("items/{productId}")]
        public async Task<ActionResult<CartResponse>> UpdateCartItem(string productId, [FromBody] UpdateCartItemRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetUserId();
                var cart = await _cartService.UpdateCartItemAsync(userId, productId, request.Quantity);
                return Ok(MapToCartResponse(cart));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                return StatusCode(500, new { message = "An error occurred while updating the cart item" });
            }
        }

        [HttpDelete("items/{productId}")]
        public async Task<ActionResult<CartResponse>> RemoveFromCart(string productId)
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.RemoveFromCartAsync(userId, productId);
                return Ok(MapToCartResponse(cart));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                return StatusCode(500, new { message = "An error occurred while removing the item from cart" });
            }
        }

        [HttpDelete]
        public async Task<ActionResult> ClearCart()
        {
            try
            {
                var userId = GetUserId();
                var success = await _cartService.ClearCartAsync(userId);

                if (success)
                    return Ok(new { message = "Cart cleared successfully" });

                return StatusCode(500, new { message = "Failed to clear cart" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, new { message = "An error occurred while clearing the cart" });
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult> GetCartSummary()
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.GetCartAsync(userId);

                return Ok(new
                {
                    totalItems = cart.TotalItems,
                    totalAmount = cart.TotalAmount,
                    itemCount = cart.Items.Count,
                    updatedAt = cart.LastModified
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart summary");
                return StatusCode(500, new { message = "An error occurred while retrieving cart summary" });
            }
        }

        [HttpPost("merge")]
        public async Task<ActionResult<CartResponse>> MergeCart([FromBody] MergeCartRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.GuestSessionId))
                    return BadRequest(new { message = "Guest session ID is required" });

                var userId = GetUserId();
                var guestUserId = $"guest:{request.GuestSessionId}";
                var cart = await _cartService.MergeCartsAsync(guestUserId, userId);
                return Ok(MapToCartResponse(cart));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging carts");
                return StatusCode(500, new { message = "An error occurred while merging carts" });
            }
        }

        public class MergeCartRequest
        {
            public string GuestSessionId { get; set; } = string.Empty;
        }
    }
}



