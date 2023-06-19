using AutoMapper;
using Finance.App.Domain.Models;
using Finance.App.Domain.Services;
using Finance.App.Dtos;
using Finance.App.Helpers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.App.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, IMapper mapper, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var categories = await _categoryService.ListAsync();
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
                return Ok(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all categories.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{id}", Name = "GetCategoryById")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Ok(categoryDto);
        }

        [HttpGet("operations/{categoryId}", Name = "GetOperationsByCategoryId")]
        [ProducesResponseType(typeof(IEnumerable<FinancialOperationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOperationsByCategoryId(int categoryId)
        {
            try
            {
                var operations = await _categoryService.GetOperationsByCategoryId(categoryId);
                if (operations == null)
                {
                    return NotFound($"No operations found for category with id {categoryId}.");
                }
                var operationsDto = _mapper.Map<IEnumerable<FinancialOperationDto>>(operations);
                return Ok(operationsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while getting operations for category with id {categoryId}.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostAsync([FromBody] SaveCategoryDto dto)
        {
            try
            {
                _logger.LogInformation("Received a request to create a new category.");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Invalid model state: {0}", ModelState.GetErrorMessages());
                    return BadRequest(ModelState.GetErrorMessages());
                }

                var category = _mapper.Map<SaveCategoryDto, Category>(dto);

                _logger.LogInformation("Saving the new category with name '{CategoryName}'", category.Name);
                var result = await _categoryService.SaveAsync(category);

                if (!result.Success)
                {
                    _logger.LogWarning($"Failed to save the new category. Reason: {0}", result.Message);
                    return BadRequest(result.Message);
                }

                var categoryDto = _mapper.Map<Category, CategoryDto>(result.Category);

                _logger.LogInformation($"New category with id '{categoryDto.Id}' and name '{categoryDto.Name}' created successfully.",
                    categoryDto.Id, categoryDto.Name);
                return CreatedAtRoute("GetCategoryById", new { id = categoryDto.Id }, categoryDto);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new category. ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutAsync(int id, [FromBody] SaveCategoryDto dto)
        {
            try
            {
                _logger.LogInformation($"Updating category with id {id}");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Invalid ModelState: {ModelState.GetErrorMessages()}");
                    return BadRequest(ModelState.GetErrorMessages());
                }

                var category = _mapper.Map<SaveCategoryDto, Category>(dto);
                var result = await _categoryService.UpdateAsync(id, category);
                if (!result.Success)
                {
                    if (result.Category == null)
                    {
                        _logger.LogWarning($"Category with id {id} not found.");
                        return NotFound($"Category with id {id} not found.");
                    }
                    _logger.LogWarning($"Failed to update category: {result.Message}");
                    return BadRequest(result.Message);
                }

                var categoryDto = _mapper.Map<Category, CategoryDto>(result.Category);
                _logger.LogInformation($"Category with id {id} updated successfully");
                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while updating category with id {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting category with id {id}"); var result = await _categoryService.DeleteAsync(id);

                if (!result.Success)
                {
                    if (result.Category == null)
                    {
                        _logger.LogWarning($"Category with id {id} not found.");
                        return NotFound($"Category with id {id} not found.");
                    }
                    _logger.LogWarning($"Failed to delete category: {result.Message}");
                    return BadRequest(result.Message);
                }

                var categoryDto = _mapper.Map<Category, CategoryDto>(result.Category);
                _logger.LogInformation($"Category with id {id} deleted successfully");
                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while deleting category with id {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
