using Ecommerce.Application.DTOs; // All your DTOs (CreateProductRequest, UpdateProductRequest, ProductDto)
using Ecommerce.Application.Interfaces; // IProductService
using Ecommerce.Application.Models; // Result, Pagination
using Microsoft.AspNetCore.Authorization; // If needed for [Authorize]
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks; // For Task

namespace Ecommerce.WebAPI.Controllers
{
    // Inherit from BaseApiController
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET all products (without explicit search/category filters in this endpoint's signature)
        // If GetAllProductsAsync needs filters, they should be parameters here.
        [HttpGet] // Route: api/Products
        public async Task<IActionResult> GetAll()
        {
            var result = await _productService.GetAllProductsAsync();
            return CreatedResponse(result); // Using CreatedResponse to map service result to HTTP response
        }

        // GET product by ID
        [HttpGet("{id}")] // Route: api/Products/{id}
        public async Task<IActionResult> Get(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            return CreatedResponse(result); // Will return 200 OK or 404 Not Found
        }

        // POST: Create a new product with image upload
        [HttpPost] // Route: api/Products
        [Consumes("multipart/form-data")] // Essential for accepting files
        public async Task<IActionResult> Create([FromForm] CreateProductRequest request) // Use CreateProductRequest
        {
            // Model state validation from DataAnnotations (e.g., [Required], [StringLength])
            if (!ModelState.IsValid)
            {
                // If model state is invalid, return a 400 Bad Request with validation errors
                // Note: Result<T> expects a message, so converting ModelState errors to a string.
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(Result<ProductDto>.BadRequest(string.Join("; ", errors)));
            }

            var result = await _productService.AddProductAsync(request);

            // Assuming ProductService returns Result<ProductDto> with StatusCode 201 on success
            // or 400/500 on failure, which CreatedResponse will then map.
            return CreatedResponse(result);
        }

        // PUT: Update an existing product with optional image upload
        [HttpPut("{id}")] // Route: api/Products/{id}
        [Consumes("multipart/form-data")] // Essential if image can be updated
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductRequest request) // Use UpdateProductRequest
        {
            // Consistency check: ensure route ID matches request body ID
            if (id != request.Id)
            {
                return BadRequest(Result<ProductDto>.BadRequest("Product ID in URL does not match ID in request body."));
            }

            // Model state validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(Result<ProductDto>.BadRequest(string.Join("; ", errors)));
            }

            var result = await _productService.UpdateProductAsync(request);

            // Assuming ProductService returns Result<ProductDto> with StatusCode 200 on success
            // or 404/400/500 on failure.
            return CreatedResponse(result); // CreatedResponse will return 200 OK or other errors
        }

        // DELETE product by ID
        [HttpDelete("{id}")] // Route: api/Products/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);

            // Assuming ProductService returns Result<bool> with StatusCode 204 on success
            // or 404/500 on failure.
            return CreatedResponse(result); // CreatedResponse will return 204 No Content or other errors
        }

        // GET paginated products with search/filter (using FromQuery for Pagination DTO)
        [HttpGet("paginated")] // Route: api/Products/paginated
        public async Task<IActionResult> GetPaginated([FromQuery] Pagination pagination)
        {
            // Model state validation for Pagination DTO
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(Result<List<ProductDto>>.BadRequest(string.Join("; ", errors)));
            }

            var result = await _productService.GetPaginatedProductsAsync(pagination);
            return CreatedResponse(result); // Will return 200 OK or 500 Internal Server Error
        }
    }
}