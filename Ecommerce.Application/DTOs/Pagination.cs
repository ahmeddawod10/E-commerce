using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs
{
    public class Pagination
    {
        public int PageNumber { get; set; } = 1; 
        public int PageSize { get; set; } = 10;
        public string? SearchQuery { get; set; }

    }

    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
    }

}
