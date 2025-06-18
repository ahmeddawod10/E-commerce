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
using Ecommerce.Application.DTOs;

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
  

        [HttpGet]  
        public async Task<IActionResult> GetCart()
        {
            string userId = GetUserId();
             
            var cartResult = await _cartService.GetCartAsync(userId);
            return CreatedResponse(cartResult);  
        }

        [HttpPost] 
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            string userId = GetUserId();
             
            var addResult = await _cartService.AddToCartAsync(userId, request);
            return CreatedResponse(addResult);
        }

        [HttpPut("{productId}")]  
        public async Task<IActionResult> UpdateCartItem(int productId, [FromQuery] int quantity)
        {
            string userId = GetUserId();

            var updateResult = await _cartService.UpdateCartItemAsync(userId, productId, quantity);
            return CreatedResponse(updateResult);
        }

        [HttpDelete("{productId}")]  
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            string userId = GetUserId();

            var removeResult = await _cartService.RemoveFromCartAsync(userId, productId);
            return CreatedResponse(removeResult);
        }

        [HttpDelete("clear")]  
        public async Task<IActionResult> ClearCart()
        {
            string userId = GetUserId();
            
            var clearResult = await _cartService.ClearCartAsync(userId);
            return CreatedResponse(clearResult);
        }

        [HttpPut("expiration")]  
        public async Task<IActionResult> SetCartExpiration([FromQuery] int days)
        {
            string userId = GetUserId();

            var expiration = TimeSpan.FromDays(days);
            var expirationResult = await _cartService.SetCartExpirationAsync(userId, expiration);
            return CreatedResponse(expirationResult);
        }
    }
}