using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;
 
namespace Ecommerce.Domain.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductByCategoryAsync(string category);
        Task<IEnumerable<Product>> GetAllWithCategoryAsync();
        Task<IEnumerable<Product>> GetPaginatedProductsAsync(int pageNumber, int pageSize);
        Task<List<Product>> GetProductsByIdsAsync(List<int> ids);



    }

}
