using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Models;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces
{
    public interface IOrderService
    {
        Task<Result<IEnumerable<OrderDto>>> GetAllOrdersAsync();
        Task<Result<OrderDto>> GetOrderByIdAsync(string id);
        Task<Result<OrderDto>> AddOrderAsync(CreateOrderDto orderDto, string userId);
        Task<Result<OrderDto>> UpdateOrderAsync(UpdateOrderDto orderDto, int id);
        Task<Result<bool>> DeleteOrderAsync(int id);
        Task<PaginatedResult<OrderDto>> GetPaginatedOrdersAsync(Pagination pagination);
    }
}
