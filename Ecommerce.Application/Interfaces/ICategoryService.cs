using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Models;

namespace Ecommerce.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<IEnumerable<CategoryDto>>> GetAllCategoryAsync();
        Task<Result<CategoryDto>> GetCategoryByIdAsync(int id);
        Task<Result<CategoryDto>> AddCategoryAsync(CategoryDto category);
        Task<Result<CategoryDto>> UpdateCategoryAsync(UpdateCategoryDto category,int id);
        Task<Result<bool>> DeleteCategoryAsync(int id);
        Task<PaginatedResult<CategoryDto>> GetPaginatedCategoriesAsync(Pagination pagination);
    }
}
