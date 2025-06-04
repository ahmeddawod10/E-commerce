using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public class ProductInfoDto
    {
        [Required]
        public string ProductId { get; set; }

        [Required]
        public string Name { get; set; }

        public decimal Price { get; set; } // Price per unit

        public int Quantity { get; set; } // Requested quantity

        public decimal TotalPrice { get; set; } // Calculated: Price * Quantity

        public string Description { get; set; } // Optional: Additional product details

        public string ImageUrl { get; set; } // Optional: Product image
    }
}
