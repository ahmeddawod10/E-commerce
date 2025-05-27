using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using System.Security.Claims;

namespace Ecommerce.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : BaseApiController
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return CreatedResponse(result);
        }

        [HttpGet("Search{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return CreatedResponse(result);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] CreateOrderDto orderDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _orderService.AddOrderAsync(orderDto, userId);
            return CreatedResponse(result);
        }

        [HttpPut("Update{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderDto orderDto)
        {
            var result = await _orderService.UpdateOrderAsync(orderDto, id);
            return CreatedResponse(result);
        }

        [HttpDelete("Delete{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _orderService.DeleteOrderAsync(id);
            return CreatedResponse(result);
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetPaginated([FromQuery] Pagination pagination)
        {
            var result = await _orderService.GetPaginatedOrdersAsync(pagination);
            return Ok(result);
        }
    }

}
