using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;


namespace Ecommerce.Domain.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<Order> GetOrderWithItemsAsync(string orderId);
        //Task<PaginatedResult<OrderDto>> GetPaginatedOrdersAsync(Pagination pagination);
    }
}
