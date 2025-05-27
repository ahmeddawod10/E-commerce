using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Models;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Services
{
    public class CategoryService: ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<CategoryDto>>> GetAllCategoryAsync()
        {
            var category = await _unitOfWork.Repository<Category>().GetAllAsync();
            var mapped=_mapper.Map<IEnumerable<CategoryDto>>(category);
            return   Result<IEnumerable<CategoryDto>>.Ok(mapped,"Category Fetched successffully");
        }

        public async Task<Result<CategoryDto>> GetCategoryByIdAsync(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            var mapped =_mapper.Map<CategoryDto>(category);
            return Result<CategoryDto>.Ok(mapped, "Cateogry fetshed successffully");
        }

        public async Task<Result<CategoryDto>> AddCategoryAsync(CategoryDto categoryDto)
        {
            var category = _mapper.Map<Category>(categoryDto);
            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.CompleteAsync();
            var mapper = _mapper.Map<CategoryDto>(category);
            return Result<CategoryDto>.Ok(mapper, "Category Added Successfuly"); 
        }

        public async Task<Result<CategoryDto>> UpdateCategoryAsync(UpdateCategoryDto categoryDto,int id)
        {
            var categoryRepo = _unitOfWork.Repository<Category>();

            var existingCategory = await categoryRepo.GetByIdAsync(id);
            if (existingCategory == null)
                return Result<CategoryDto>.NotFound("Category not found.");

            _mapper.Map(categoryDto, existingCategory);

            categoryRepo.Update(existingCategory);
            await _unitOfWork.CompleteAsync();

            var updatedDto = _mapper.Map<CategoryDto>(existingCategory);
            return Result<CategoryDto>.Ok(updatedDto, "Category updated successfully.");
        }

        public async Task<Result<bool>> DeleteCategoryAsync(int id)
        {
            var categoryRepo = _unitOfWork.Repository<Category>();
            var category = await categoryRepo.GetByIdAsync(id);

            if (category == null)
            {
                return Result<bool>.NotFound("Category not found");
            }

            _unitOfWork.Repository<Category>().Delete(category);
            await _unitOfWork.CompleteAsync();
            return Result<bool>.Ok(true, "Deleted Successfully");
        }
        public async Task<PaginatedResult<CategoryDto>> GetPaginatedCategoriesAsync(Pagination pagination)
        {
            var query = _unitOfWork.Repository<Category>().AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToListAsync();

            return new PaginatedResult<CategoryDto>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

    }
}