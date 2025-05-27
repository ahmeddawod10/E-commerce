using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs
{
    public class FavouriteDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
    public class FavoriteDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }

        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductImageUrl { get; set; }
    }

    public class AddFavoriteDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }

    }
}
