using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        //ICategoryRepository Category { get; }
        IGenericRepository<T> Repository<T>() where T : class;
        Task<int> CompleteAsync(); 
    }

}
