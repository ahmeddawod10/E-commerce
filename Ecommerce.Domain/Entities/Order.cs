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
        [Key]
        public string Id { get; set; }  

        [Required]
        public string UserId { get; set; } 

        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }

        public Address ShippingAddress { get; set; }
        public Address BillingAddress { get; set; }  

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public decimal SubTotal { get; set; }   
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }  

        public string? PaymentIntentId { get; set; }  
        public DateTime LastModified { get; set; }

        public Order()
        {
            Id = Guid.NewGuid().ToString();  
            OrderDate = DateTime.UtcNow;
            LastModified = DateTime.UtcNow;
            Status = OrderStatus.Pending;
        }
    }

}
