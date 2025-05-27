using AutoMapper;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService) => _productService = productService;

        [HttpGet("get all")]
         public async Task<IActionResult> GetAll(string ? searchByName , int categoryId)
            => Ok(await _productService.GetAllProductsAsync());

        [HttpGet("get{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return CreatedResponse(product);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ProductDto product)
        {
            var r =await _productService.AddProductAsync(product);
            return CreatedResponse(r);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] ProductDto product)
        {
            var r = await _productService.UpdateProductAsync(product);
            return CreatedResponse(r);
        }

        [HttpDelete("delete{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _productService.DeleteProductAsync(id);
            return CreatedResponse(r);
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetPaginated([FromQuery] Pagination pagination)
        {
            var result = await _productService.GetPaginatedProductsAsync(pagination);
            return Ok(result);
        }


    }



}
