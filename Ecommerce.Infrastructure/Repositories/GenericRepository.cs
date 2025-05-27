using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        public GenericRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<T>> GetAllAsync() => await _context.Set<T>().ToListAsync();

        public async Task<T?> GetByIdAsync(int id) => await _context.Set<T>().FindAsync(id);

        public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);

        public void Update(T entity) => _context.Set<T>().Update(entity);

        public void Delete(T entity) => _context.Set<T>().Remove(entity);


        public IQueryable<T> AsQueryable()
        {
            return _context.Set<T>().AsQueryable();
        }

        public async Task<IEnumerable<T>> GetPaginatedProductsAsync(int pageNumber, int pageSize)
        {
            return await _context.Set<T>()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<PaginatedResult<ProductDto>> GetPaginatedCategoriesAsync(Pagination pagination)
        {
            var query = _context.Products.AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name
                }).ToListAsync();

            return new PaginatedResult<ProductDto>
            {
                Items = items,
                TotalCount = totalCount
            };
        }


        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(OrderDto orderDto, Pagination pagination)
        {
            return await _context.Orders
                .Where (o => o.UserId.Equals( orderDto.UserId))
                .OrderByDescending(o => o.OrderDate)  
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();
        }
    }


}
