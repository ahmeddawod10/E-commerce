// Ecommerce.Application.Services/ProductService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Domain.Interfaces; 
using Ecommerce.Domain.Entities;    
using Ecommerce.Application.DTOs;  
using Ecommerce.Application.Interfaces;  
using Ecommerce.Application.Models;  
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FluentValidation; 
using Microsoft.Extensions.Logging; 

namespace Ecommerce.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly ILogger<ProductService> _logger;  

        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const int MaxImageSizeMb = 5;  

        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IFileService fileService,
            ILogger<ProductService> logger)
         {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<IEnumerable<ProductDto>>> GetAllProductsAsync()
        {
            try
            {
                var products = await _unitOfWork.Products.GetAllAsync();
                var mapped = _mapper.Map<IEnumerable<ProductDto>>(products);
                return Result<IEnumerable<ProductDto>>.Ok(mapped, "Products fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all products.");
                return Result<IEnumerable<ProductDto>>.BadRequest("Failed to retrieve products due to an internal error.");
            }
        }

        public async Task<Result<ProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return Result<ProductDto>.NotFound($"Product with ID {id} not found.");
                }

                var dto = _mapper.Map<ProductDto>(product);
                return Result<ProductDto>.Ok(dto, "Product retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve product with ID {id}.");
                return Result<ProductDto>.BadRequest($"Failed to retrieve product due to an internal error.");
            }
        }

        public async Task<Result<ProductDto>> AddProductAsync(CreateProductRequest request)
        {
            string? imageUrl = null;
            if (request.ImageFile != null)
            {
                try
                {
                    imageUrl = await _fileService.SaveFileAsync(request.ImageFile, _allowedImageExtensions, MaxImageSizeMb);
                }
                catch (ArgumentException argEx) 
                {
                    _logger.LogWarning(argEx, "Image upload validation failed for product creation.");
                    return Result<ProductDto>.BadRequest($"Image upload failed: {argEx.Message}");
                }
                catch (InvalidOperationException opEx) 
                {
                    _logger.LogError(opEx, "Failed to save product image due to server error.");
                    return Result<ProductDto>.BadRequest($"Image upload failed due to a server error: {opEx.Message}");
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, "An unexpected error occurred during image upload for product creation.");
                    return Result<ProductDto>.BadRequest($"An unexpected error occurred during image upload: {ex.Message}");
                }
            }
            try
            {
                var product = _mapper.Map<Product>(request);
                product.ImageUrl = imageUrl;

                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.CompleteAsync();

                var mappedProductDto = _mapper.Map<ProductDto>(product); 
                return Result<ProductDto>.Ok(mappedProductDto, "Product added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add product to the database.");
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    try
                    {
                        _fileService.DeleteFile(imageUrl);
                        _logger.LogInformation($"Successfully rolled back image '{imageUrl}' after DB save failure.");
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogError(deleteEx, $"Failed to roll back image '{imageUrl}' after DB save failure.");
                    }
                }
                return Result<ProductDto>.BadRequest("Failed to add product due to an internal error.");
            }
        }

        public async Task<Result<ProductDto>> UpdateProductAsync(UpdateProductRequest request)
        {
            // Optional: FluentValidation for UpdateProductRequest
            // if (_updateProductRequestValidator != null)
            // {
            //     var validationResult = await _updateProductRequestValidator.ValidateAsync(request);
            //     if (!validationResult.IsValid)
            //     {
            //         var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            //         return Result<ProductDto>.BadRequest(string.Join("; ", errors));
            //     }
            // }

            var existingProduct = await _unitOfWork.Products.GetByIdAsync(request.Id);
            if (existingProduct == null)
            {
                return Result<ProductDto>.NotFound($"Product with ID {request.Id} not found.");
            }

            string? oldImageUrl = existingProduct.ImageUrl; // Store old URL BEFORE potential update

            // Handle image update if a new file is provided
            if (request.ImageFile != null)
            {
                try
                {
                    var newImageUrl = await _fileService.SaveFileAsync(request.ImageFile, _allowedImageExtensions, MaxImageSizeMb);
                    existingProduct.ImageUrl = newImageUrl; // Update to the new image URL
                }
                catch (ArgumentException argEx)
                {
                    _logger.LogWarning(argEx, $"Image update validation failed for product ID {request.Id}.");
                    return Result<ProductDto>.BadRequest($"Image update failed: {argEx.Message}");
                }
                catch (InvalidOperationException opEx)
                {
                    _logger.LogError(opEx, $"Failed to save new image for product ID {request.Id} due to server error.");
                    return Result<ProductDto>.BadRequest($"Image update failed due to a server error: {opEx.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unexpected error occurred during image upload for product ID {request.Id}.");
                    return Result<ProductDto>.BadRequest($"An unexpected error occurred during image update: {ex.Message}");
                }
            }

            try
            {
                // Map other updated properties from request DTO to existing entity
                // Make sure your AutoMapper profile handles mapping from UpdateProductRequest to Product
                _mapper.Map(request, existingProduct);

                _unitOfWork.Products.Update(existingProduct);
                await _unitOfWork.CompleteAsync();

                // If DB update is successful and a new image was uploaded, delete the old one
                if (request.ImageFile != null && !string.IsNullOrEmpty(oldImageUrl))
                {
                    try
                    {
                        _fileService.DeleteFile(oldImageUrl);
                        _logger.LogInformation($"Successfully deleted old image '{oldImageUrl}' for product ID {existingProduct.Id}.");
                    }
                    catch (Exception ex)
                    {
                        // Log the exception but don't prevent the product update from succeeding
                        _logger.LogWarning(ex, $"Failed to delete old image file '{oldImageUrl}' for product ID {existingProduct.Id}.");
                    }
                }

                var mappedProductDto = _mapper.Map<ProductDto>(existingProduct); // Map the updated entity back to DTO
                return Result<ProductDto>.Ok(mappedProductDto, "Product updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update product ID {request.Id} in the database.");
                // If DB update fails after new image was saved, you might want to delete the newly uploaded image
                if (request.ImageFile != null && !string.IsNullOrEmpty(existingProduct.ImageUrl) && existingProduct.ImageUrl != oldImageUrl)
                {
                    try
                    {
                        _fileService.DeleteFile(existingProduct.ImageUrl); // Attempt to delete the new, unsaved image
                        _logger.LogInformation($"Successfully rolled back newly uploaded image '{existingProduct.ImageUrl}' after DB update failure.");
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogError(deleteEx, $"Failed to roll back new image '{existingProduct.ImageUrl}' after DB update failure.");
                    }
                }
                return Result<ProductDto>.BadRequest("Failed to update product due to an internal error.");
            }
        }

        public async Task<Result<bool>> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return Result<bool>.NotFound($"Product with ID {id} not found.");
                }

                _unitOfWork.Products.Delete(product);
                await _unitOfWork.CompleteAsync();

                // Delete the associated image file AFTER successful product deletion from DB
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    try
                    {
                        _fileService.DeleteFile(product.ImageUrl);
                        _logger.LogInformation($"Successfully deleted image '{product.ImageUrl}' for product ID {product.Id}.");
                    }
                    catch (Exception ex)
                    {
                        // Log the exception but don't prevent product deletion if image deletion fails
                        _logger.LogWarning(ex, $"Failed to delete image file '{product.ImageUrl}' for product ID {product.Id} after successful DB deletion.");
                    }
                }

                return Result<bool>.Ok(true, "Product deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete product with ID {id}.");
                return Result<bool>.BadRequest("Failed to delete product due to an internal error.");
            }
        }

        public async Task<Result<List<ProductDto>>> GetPaginatedProductsAsync(Pagination pagination)
        {
            try
            {
                var productsQuery = _unitOfWork.Products.AsQueryable(); // Get IQueryable

                if (!string.IsNullOrWhiteSpace(pagination.SearchQuery))
                {
                    var search = pagination.SearchQuery.Trim().ToLower();
                    productsQuery = productsQuery.Where(p =>
                        p.Name.ToLower().Contains(search) ||
                        (p.Description != null && p.Description.ToLower().Contains(search)));
                }

                // Eager load category if ProductDto displays CategoryName
                // Assuming Products repository has an include method or you can use .Include here
                // Example: productsQuery = productsQuery.Include(p => p.Category);
                // Or ensure GetAllWithCategoryAsync handles pagination if you intend to use it more broadly.

                var pagedProducts = await productsQuery
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                var productDtos = _mapper.Map<List<ProductDto>>(pagedProducts);

                return Result<List<ProductDto>>.Ok(productDtos, "Products retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve paginated products.");
                return Result<List<ProductDto>>.BadRequest("Failed to retrieve products due to an internal error.");
            }
        }
    }
}