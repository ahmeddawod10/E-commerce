using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs
{
    public class OrderItemDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }

    public class OrderDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public IList<OrderItemDto> OrderItems { get; set; }
    }

    public class CreateOrderItemDto
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateOrderDto
    {
        public IList<CreateOrderItemDto> OrderItems { get; set; }
        public decimal ShippingCost { get; set; }
    }

    public class UpdateOrderDto
    {
        public string Status { get; set; }
    }



}
