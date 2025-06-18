using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Models;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    [Authorize]
    public class FavoriteController : BaseApiController
    {
        private readonly IFavoriteService _favoriteService;
        private readonly ILogger<FavoriteController> _logger;

        public FavoriteController(IFavoriteService favoriteService, ILogger<FavoriteController> logger)
        {
            _favoriteService = favoriteService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<Favorite>> Get()
        {
            var userId = GetUserId();
            var favorites = await _favoriteService.GetFavoritesAsync(userId);
            return Ok(favorites);
        }

        [HttpPost("{productId}")]
        public async Task<ActionResult<Favorite>> Add(string productId)
        {
            var userId = GetUserId();
            var favorites = await _favoriteService.AddFavoriteAsync(userId, productId);
            return Ok(favorites);
        }

        [HttpDelete("{productId}")]
        public async Task<ActionResult<Favorite>> Remove(string productId)
        {
            var userId = GetUserId();
            var favorites = await _favoriteService.RemoveFavoriteAsync(userId, productId);
            return Ok(favorites);
        }

        [HttpDelete]
        public async Task<ActionResult> Clear()
        {
            var userId = GetUserId();
            var success = await _favoriteService.ClearFavoritesAsync(userId);

            if (success)
                return Ok(new { message = "Favorites cleared successfully" });

            return StatusCode(500, new { message = "Failed to clear favorites" });
        }
    }


}

