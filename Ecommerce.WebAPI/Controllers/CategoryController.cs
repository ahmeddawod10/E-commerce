using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : BaseApiController
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)=>_categoryService=categoryService;

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result= await _categoryService.GetAllCategoryAsync();
            return CreatedResponse(result);
        }

        [HttpGet("Search")]
        public async Task<IActionResult>GetById(int id)
        {
            var category=await _categoryService.GetCategoryByIdAsync(id);
            return CreatedResponse(category);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Create([FromBody]CategoryDto categoryDto)
        {
            var category =await _categoryService.AddCategoryAsync(categoryDto);
            return CreatedResponse(category);
        }

        [HttpPut("Update")]
        public async Task<IActionResult>Update([FromBody] UpdateCategoryDto categoryDto,int id)
        {
            //if (id != categoryDto.Id) { return BadRequest(); }
            //    var x = await _categoryService.GetCategoryByIdAsync(id);
            var category = await _categoryService.UpdateCategoryAsync(categoryDto,id);
            return CreatedResponse(category);
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult>Delete(int id)
        {
            var category=await _categoryService.DeleteCategoryAsync(id);
            return CreatedResponse(category);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories([FromQuery] Pagination pagination)
        {
            var result = await _categoryService.GetPaginatedCategoriesAsync(pagination);
            return Ok(result);
        }

    }
}
