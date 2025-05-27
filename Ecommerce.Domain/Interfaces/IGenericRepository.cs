using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        IQueryable<T> AsQueryable();

        Task<IEnumerable<T>> GetPaginatedProductsAsync(int pageNumber, int pageSize);
        //Task<IEnumerable<T>> GetOrdersByUserIdAsync(T entity, T entitys);
       // Task<IEnumerable<T>> GetOrdersByUserIdAsync(T c, T s);

 
      }

}
