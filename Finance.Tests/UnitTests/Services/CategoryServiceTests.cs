using Finance.App.Domain.Models;
using Finance.App.Domain.Repositories;
using Finance.App.Domain.Services;
using Finance.App.Domain.Services.Communication;
using Finance.App.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Finance.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<ICategoryService>> _mockLogger;
        private readonly ICategoryService _categoryService;

        public CategoryServiceTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<ICategoryService>>();
            _categoryService = new CategoryService(_mockCategoryRepository.Object, _mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCalled_ReturnsOperation()
        {
            // Arrange
            int id = 1;
            Category expectedOperation = new Category
            {
                Id = id,
                Name = "Test Operation"
            };

            _mockCategoryRepository.Setup(repo => repo.FindByIdAsync(id)).ReturnsAsync(expectedOperation);

            // Act
            var operation = await _categoryService.GetByIdAsync(id);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal(expectedOperation.Id, operation.Id);
            Assert.Equal(expectedOperation.Name, operation.Name);
        }

        [Fact]
        public async Task DeleteAsync_WhenCategoryNotFound_ReturnsCategoryResponseWithNotFoundMessage()
        {
            // Arrange
            var categoryId = 1;
            Category existingCategory = null;
            _mockCategoryRepository.Setup(x => x.FindByIdAsync(categoryId)).ReturnsAsync(existingCategory);

            // Act
            var result = await _categoryService.DeleteAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Category not Found", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_WhenCategoryFound_RemovesCategoryFromRepositoryAndCompletesUnitOfWork()
        {
            // Arrange
            var categoryId = 1;
            var existingCategory = new Category { Id = categoryId, Name = "Test Category" };
            _mockCategoryRepository.Setup(x => x.FindByIdAsync(categoryId)).ReturnsAsync(existingCategory);

            // Act
            var result = await _categoryService.DeleteAsync(categoryId);

            // Assert
            _mockCategoryRepository.Verify(x => x.Remove(existingCategory), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(existingCategory, result.Category);
        }

        [Fact]
        public async Task GetOperationsByCategoryId_WhenCategoryNotFound_ReturnsNull()
        {
            // Arrange
            var categoryId = 1;
            Category category = null; // Set category to null to simulate category not found
            _mockCategoryRepository.Setup(r => r.FindByIdAsync(categoryId)).ReturnsAsync(category);

            // Act
            var result = await _categoryService.GetOperationsByCategoryId(categoryId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOperationsByCategoryId_WhenCalled_ReturnsListOfOperations()
        {
            // Arrange
            var categoryId = 1;
            var expectedOperations = new List<FinancialOperation>
            {
                new FinancialOperation { Id = 1, CategoryId = categoryId },
                new FinancialOperation { Id = 2, CategoryId = categoryId }
            };
            _mockCategoryRepository.Setup(r => r.FindByIdAsync(categoryId)).ReturnsAsync(new Category());
            _mockCategoryRepository.Setup(r => r.GetOperationsByCategoryId(categoryId)).ReturnsAsync(expectedOperations);

            // Act
            var result = await _categoryService.GetOperationsByCategoryId(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<FinancialOperation>>(result);
            Assert.Equal(expectedOperations.Count, result.Count());
            foreach (var operation in expectedOperations)
            {
                Assert.Contains(result, r => r.Id == operation.Id);
            }
        }

        [Fact]
        public async Task SaveAsync_WhenCategoryIsValid_ReturnsCategoryResponseWithId()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };
            _mockCategoryRepository.Setup(r => r.FindByIdAsync(category.Id)).ReturnsAsync(new Category());
            _mockCategoryRepository.Setup(r => r.AddAsync(category)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _categoryService.SaveAsync(category);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Category.Id > 0);
            _mockCategoryRepository.Verify(r => r.AddAsync(category), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task SaveAsync_WhenCalledWithValidCategory_ReturnsCategoryResponseWithSavedCategory()
        {
            // Arrange
            var categoryToSave = new Category { Id = 1, Name = "Test Category" };
            var savedCategory = new Category { Id = 1, Name = "Test Category" };
            _mockCategoryRepository.Setup(r => r.AddAsync(categoryToSave)).Returns(Task.FromResult(savedCategory));

            // Act
            var result = await _categoryService.SaveAsync(categoryToSave);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CategoryResponse>(result);
            Assert.Equal(savedCategory.Id, result.Category.Id);
            Assert.Equal(savedCategory.Name, result.Category.Name);
            _mockCategoryRepository.Verify(r => r.AddAsync(categoryToSave), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenCalledWithValidCategory_ReturnsUpdatedCategoryResponse()
        {
            // Arrange
            var categoryToUpdate = new Category { Id = 1, Name = "Test Category" };
            var updatedCategory = new Category { Id = 1, Name = "Updated Test Category" };
            _mockCategoryRepository.Setup(r => r.FindByIdAsync(categoryToUpdate.Id)).ReturnsAsync(categoryToUpdate);
            _mockCategoryRepository.Setup(r => r.Update(updatedCategory)).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
            _mockCategoryRepository.Setup(r => r.FindByIdAsync(updatedCategory.Id)).ReturnsAsync(updatedCategory);

            // Act
            var result = await _categoryService.UpdateAsync(categoryToUpdate.Id, updatedCategory);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CategoryResponse>(result);
            Assert.Equal(updatedCategory.Id, result.Category.Id);
            Assert.Equal(updatedCategory.Name, result.Category.Name);
            _mockCategoryRepository.Verify(r => r.FindByIdAsync(categoryToUpdate.Id), Times.Once);
            _mockCategoryRepository.Verify(r => r.Update(updatedCategory), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}
