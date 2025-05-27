using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.DTOs;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductByCategoryAsync(string categoryName)
        {
            return await _context.Products
         
                         .Include(p => p.Category)
                         .Where(p => p.Category.Name == categoryName)
                         .ToListAsync(); 
        }

        public async Task<IEnumerable<Product>> GetAllWithCategoryAsync()
        {
            return await _context.Products
                                 .Include(p => p.Category)
                                 .ToListAsync();
        }

        public async Task<PaginatedResult<ProductDto>> GetPaginatedProductsAsync(Pagination pagination)
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

        public async Task<List<Product>> GetProductsByIdsAsync(List<string> ids)
        {
            var intIds = ids.Select(id => int.Parse(id)).ToList();

            return await _context.Products
                .Where(p => intIds.Contains(p.Id))
                .ToListAsync();
        }

    }

}
