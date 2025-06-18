using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Domain.Entities
{
    public class CartItem
    {
        public int ProductId { get; set; } 
        public decimal? Price { get; set; }
        public int Quantity { get; set; }
        public decimal? TotalPrice => Price * Quantity;

    }
}
