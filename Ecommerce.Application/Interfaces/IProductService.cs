using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Models;


namespace Ecommerce.Application.Interfaces
{
    public interface IProductService
    {
        Task<Result<IEnumerable<ProductDto>>> GetAllProductsAsync();    
        Task<Result<ProductDto>> GetProductByIdAsync(int id);
        //Task<Result<ProductDto>>AddProductAsync(ProductDto product);
        //Task<Result<ProductDto>>UpdateProductAsync(ProductDto product);
        public Task<Result<ProductDto>> AddProductAsync(CreateProductRequest request);
        public Task<Result<ProductDto>> UpdateProductAsync(UpdateProductRequest request);
        Task<Result<bool>> DeleteProductAsync(int id);
        Task<Result<List<ProductDto>>> GetPaginatedProductsAsync(Pagination pagination);
    }
}
