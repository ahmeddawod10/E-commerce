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
        public string UserId { get; set; } = string.Empty;
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal? TotalAmount => Items.Sum(item => item.TotalPrice);
        public int? TotalItems => Items.Sum(item => item.Quantity);
       

    }

   }



