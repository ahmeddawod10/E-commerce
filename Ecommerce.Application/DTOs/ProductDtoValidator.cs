using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Ecommerce.Application.DTOs
{

    public class ProductDtoValidator : AbstractValidator<ProductDto>
    {
        public ProductDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.Stock).LessThan(1000).WithMessage("must be less than 1000");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than zero");
        }
    }
}
