using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Application.DTOs;
using Ecommerce.Domain.Entities;
using AutoMapper;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Models;

namespace Ecommerce.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ProductDto>>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            var mapped = _mapper.Map<IEnumerable<ProductDto>>(products);
            return Result<IEnumerable<ProductDto>>.Ok(mapped,"Products fetched successfully.");  
        }

        public async Task<Result<ProductDto>> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            var mapped = _mapper.Map<ProductDto>(product);
            return Result<ProductDto>.Ok(mapped, "Product fetshed successfuly.");
        }

        public async Task<Result<ProductDto>> AddProductAsync(ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();
            var mapped = _mapper.Map<ProductDto>(product);

            return Result<ProductDto>.Ok(mapped, "Product added successfully.");
        }


        public async Task<Result<ProductDto>> UpdateProductAsync(ProductDto productDto)
        {
            var existingProduct = await _unitOfWork.Products.GetByIdAsync(productDto.Id);
            if (existingProduct == null)
                return Result< ProductDto >.NotFound("Product not found"); ;

            var product = _mapper.Map<Product>(productDto);
            var mapped = _mapper.Map<ProductDto>(product);
            _unitOfWork.Products.Update(product);
            await _unitOfWork.CompleteAsync();

            return Result<ProductDto>.Ok(mapped,"Product updated successfully.");

        }

        public async Task<Result<bool>> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
             return 
                    Result<bool>.NotFound("Product not found");
            }
            _unitOfWork.Products.Delete(product);
            await _unitOfWork.CompleteAsync();
            return Result<bool>.Ok(true,"Deleted Successfully");
        }
    }



}
