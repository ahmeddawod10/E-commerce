using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Domain.Entities
{
    public class Cart
    {
        //public int Id { get; set; }
        //public string UserId { get; set; }
        //public int ProductId { get; set; }
        //public int Quantity { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //public ApplicationUser User { get; set; }
        //public Product Product { get; set; }
        public string UserId { get; set; } = string.Empty;
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount => Items.Sum(item => item.TotalPrice);
        public int TotalItems => Items.Sum(item => item.Quantity);



        // DTOs for API requests/responses
        public class AddToCartRequest
        {
            [Required]
            public string ProductId { get; set; } = string.Empty;

            [Required]
            public string ProductName { get; set; } = string.Empty;

            [Required]
            [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
            public decimal Price { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int Quantity { get; set; }

            public string? ImageUrl { get; set; }
            public Dictionary<string, string>? ProductAttributes { get; set; }
        }

        public class UpdateCartItemRequest
        {
            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int Quantity { get; set; }
        }

        public class CartResponse
        {
            public string UserId { get; set; } = string.Empty;
            public List<CartItem> Items { get; set; } = new();
            public decimal TotalAmount { get; set; }
            public int TotalItems { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }

}

