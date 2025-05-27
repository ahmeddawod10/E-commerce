using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImgURL { get; set; }

        public int Price { get; set; }
        public int Stock { get; set; }
        public int? CategoryId { get; set; }
        //public string? Category { get; set; }
    }
}
