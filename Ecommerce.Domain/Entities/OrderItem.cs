using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Domain.Entities
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; } // Database auto-incremented PK

        [Required]
        public string ProductId { get; set; } // The ID of the product from your catalog
        [Required]
        public string ProductName { get; set; } // Snapshot of product name at time of order
        public string? ProductImageUrl { get; set; } // Snapshot of image URL

        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; } // Snapshot of price at time of order

        public string OrderId { get; set; } // Foreign Key
        public virtual Order Order { get; set; }
    }
}
