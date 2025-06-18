using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
       // public string? ImageName { get; set; }
        public string? ImageUrl { get; set; }
        public int Price { get; set; }
        public int Stock { get; set; }
        public int? CategoryId { get; set; }
    }
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Product name is required.")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public int? CategoryId { get; set; }

        public IFormFile? ImageFile { get; set; } 
    }

    public class UpdateProductRequest
    {
        [Required]
        public int Id { get; set; } 

        [Required(ErrorMessage = "Product name is required.")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; } 

        public int Stock { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public int? CategoryId { get; set; }  

        public IFormFile? ImageFile { get; set; } 
    }


}
