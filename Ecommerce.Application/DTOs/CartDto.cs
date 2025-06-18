using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs
{
    public class CartDto
    {
        public string UserId { get; set; }
        public int TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public List<CartItemDtos> Items { get; set; } = new List<CartItemDtos>();

    }

    public class CartItemDtos
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string ImgUrl { get; set; }
        public int Quantity { get; set; }

    }


    public class AddToCartRequest
    {
        [Required]
        public int ProductId { get; set; }

        
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

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
