using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Domain.Entities
{
    public enum OrderStatus
    {
        Pending,        // Order placed, awaiting payment/processing
        Processing,     // Payment received, order being processed
        Shipped,        // Order has been shipped
        Delivered,      // Order has been delivered
        Cancelled,      // Order was cancelled
        Refunded        // Order was refunded
    }
}
