using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Domain.Entities
{
    public class Favorite
    {
        public string UserId { get; set; } = string.Empty;
        public List<FavoriteItem> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class FavoriteItem
    {
        public string ProductId { get; set; } = string.Empty;
    }
}
