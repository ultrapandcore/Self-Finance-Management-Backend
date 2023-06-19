using Finance.App.Domain.Models;
using Finance.App.Domain.Repositories;
using Finance.App.Domain.Services;
using Finance.App.Domain.Services.Communication;

namespace Finance.App.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ICategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, ILogger<ICategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CategoryResponse> DeleteAsync(int id)
        {
            var existingCategory = await _categoryRepository.FindByIdAsync(id);

            if (existingCategory == null)
            {
                return new CategoryResponse("Category not Found");
            }

            try
            {
                _categoryRepository.Remove(existingCategory);
                await _unitOfWork.CompleteAsync();

                return new CategoryResponse(existingCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred when deleting the category with ID {id}");
                return new CategoryResponse($"An error occurred when deleting the category: {ex.Message}");
            }
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _categoryRepository.FindByIdAsync(id);
        }

        public async Task<IEnumerable<FinancialOperation>> GetOperationsByCategoryId(int id)
        {
            var category = await _categoryRepository.FindByIdAsync(id);

            if (category == null)
            {
                return null;
            }

            return await _categoryRepository.GetOperationsByCategoryId(id);
        }

        public async Task<IEnumerable<Category>> ListAsync()
        {
            return await _categoryRepository.ListAsync();
        }

        public async Task<CategoryResponse> SaveAsync(Category category)
        {
            var existingCategory = await _categoryRepository.FindByNameAsync(category.Name);
            if (existingCategory != null)
            {
                _logger.LogError($"A category with the name '{category.Name}' already exists");
                return new CategoryResponse($"A category with the name '{category.Name}' already exists");
            }

            try
            {
                await _categoryRepository.AddAsync(category);
                await _unitOfWork.CompleteAsync();

                return new CategoryResponse(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred when saving the category {ex.Message}");
                return new CategoryResponse($"An error occurred when saving the category: {ex.Message}");
            }
        }

        public async Task<CategoryResponse> UpdateAsync(int id, Category category)
        {
            var existingCategory = await _categoryRepository.FindByIdAsync(id);

            if (existingCategory == null)
            {
                return new CategoryResponse("Category not found.");
            }

            // Check if a category with the same name already exists
            var duplicateCategory = await _categoryRepository.FindByNameAsync(category.Name);
            if (duplicateCategory != null && duplicateCategory.Id != id)
            {
                _logger.LogError($"A category with the name '{category.Name}' already exists");
                return new CategoryResponse($"A category with the name '{category.Name}' already exists");
            }

            existingCategory.Name = category.Name;

            try
            {
                _categoryRepository.Update(existingCategory);
                await _unitOfWork.CompleteAsync();

                return new CategoryResponse(existingCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred when deleting the category with ID {id}");
                return new CategoryResponse($"An error occurred when updating the category: {ex.Message}");
            }
        }
    }
}
