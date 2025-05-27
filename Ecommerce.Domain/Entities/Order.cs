using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Domain.Entities
{
    public class Order
    {
        //public int Id { get; set; }
        //public string UserId { get; set; }
        //public decimal TotalAmount { get; set; }
        //public string Status { get; set; } = "Pending";
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //public ApplicationUser User { get; set; }
        //public IEnumerable<OrderItem> Items { get; set; }
        [Key]
        public string Id { get; set; } // Could be Guid.ToString() or database generated

        [Required]
        public string UserId { get; set; }
        // Navigation property for User if you have a User entity managed by EF Core
        // public virtual User User { get; set; }

        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }

        public Address ShippingAddress { get; set; }
        public Address BillingAddress { get; set; } // Could be same as shipping

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public decimal SubTotal { get; set; } // Calculated from OrderItems
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; } // SubTotal + ShippingCost + TaxAmount

        public string? PaymentIntentId { get; set; } // For linking with payment gateway like Stripe
        public DateTime LastModified { get; set; }

        public Order()
        {
            Id = Guid.NewGuid().ToString(); // Default ID generation
            OrderDate = DateTime.UtcNow;
            LastModified = DateTime.UtcNow;
            Status = OrderStatus.Pending;
        }
    }

}
