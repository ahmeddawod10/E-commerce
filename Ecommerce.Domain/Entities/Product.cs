using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        //public string? ImageName { get; set; }
        public string? ImageUrl { get; set; } 
        public int Price { get; set; }
        public int Stock { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
