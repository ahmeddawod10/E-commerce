using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(20, ErrorMessage = "Name must be less than 20 chars")]    ///Data Annotations
        public string? Name { get; set; }
        public string? Description { get; set; }
        //public IEnumerable<Product> Products { get; set; }

    }
}
