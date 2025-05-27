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

namespace Ecommerce.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<OrderDto>>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork.Repository<Order>()
                .AsQueryable()
                .Include(o => o.OrderItems)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<OrderDto>>(orders);
            return Result<IEnumerable<OrderDto>>.Ok(mapped, "Orders fetched successfully");
        }

        public async Task<Result<OrderDto>> GetOrderByIdAsync(string id)
        {
            var order = await _unitOfWork.Repository<Order>()
                .AsQueryable()
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return Result<OrderDto>.NotFound("Order not found.");

            var mapped = _mapper.Map<OrderDto>(order);
            return Result<OrderDto>.Ok(mapped, "Order fetched successfully");
        }

        public async Task<Result<OrderDto>> AddOrderAsync(CreateOrderDto orderDto, string userId)
        {
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                ShippingCost = orderDto.ShippingCost,
                OrderItems = orderDto.OrderItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = "TODO: fetch real product name",  
                    Quantity = item.Quantity,
                    PriceAtPurchase = 0m  
                }).ToList()
            };

            order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.PriceAtPurchase) + order.ShippingCost;

            await _unitOfWork.Repository<Order>().AddAsync(order);
            await _unitOfWork.CompleteAsync();

            var mapped = _mapper.Map<OrderDto>(order);
            return Result<OrderDto>.Ok(mapped, "Order placed successfully");
        }

        public async Task<Result<OrderDto>> UpdateOrderAsync(UpdateOrderDto orderDto, int id)
        {
            var repo = _unitOfWork.Repository<Order>();
            var existingOrder = await repo.GetByIdAsync(id);

            if (existingOrder == null)
                return Result<OrderDto>.NotFound("Order not found.");

            if (!Enum.TryParse<OrderStatus>(orderDto.Status, out var newStatus))
                return Result<OrderDto>.BadRequest("Invalid order status.");

            existingOrder.Status = newStatus;
            repo.Update(existingOrder);
            await _unitOfWork.CompleteAsync();

            var updatedDto = _mapper.Map<OrderDto>(existingOrder);
            return Result<OrderDto>.Ok(updatedDto, "Order updated successfully.");
        }

        public async Task<Result<bool>> DeleteOrderAsync(int id)
        {
            var repo = _unitOfWork.Repository<Order>();
            var order = await repo.GetByIdAsync(id);

            if (order == null)
                return Result<bool>.NotFound("Order not found.");

            repo.Delete(order);
            await _unitOfWork.CompleteAsync();
            return Result<bool>.Ok(true, "Order deleted successfully");
        }

        public async Task<PaginatedResult<OrderDto>> GetPaginatedOrdersAsync(Pagination pagination)
        {
            var query = _unitOfWork.Repository<Order>().AsQueryable().Include(o => o.OrderItems);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            var mappedItems = _mapper.Map<List<OrderDto>>(items);

            return new PaginatedResult<OrderDto>
            {
                Items = mappedItems,
                TotalCount = totalCount
            };
        }
    }
}
