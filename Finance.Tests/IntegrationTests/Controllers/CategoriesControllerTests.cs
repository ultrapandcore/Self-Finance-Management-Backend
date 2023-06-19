using Finance.App.Dtos;
using Finance.App.Helpers;
using System.Net;
using Xunit;

namespace Finance.IntegrationTests.Controllers
{
    public class CategoriesControllerTests : BaseControllerTests
    {
        [Fact]
        public async Task GetAllAsync_ShouldReturnNonEmptyListOfCategories()
        {
            var (categories, statusCode) = await SendRequestAsync<IEnumerable<CategoryDto>>(HttpMethod.Get, ApiRoutes.Categories);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.NotEmpty(categories);
            Assert.Equal(4, categories.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCategoryWithSpecifiedId()
        {
            var (category, statusCode) = await SendRequestAsync<CategoryDto>(HttpMethod.Get, $"{ApiRoutes.Categories}1");
            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.NotNull(category);
            Assert.Equal(1, category.Id);
            Assert.Equal("Shopping", category.Name);
        }

        [Fact]
        public async Task GetOperationsByCategoryId_ShouldReturnNonEmptyListOfOperationsForSpecifiedCategory()
        {
            var (operations, statusCode) = await SendRequestAsync<IEnumerable<FinancialOperationDto>>(HttpMethod.Get, $"{ApiRoutes.OperationsByCategories}1");
            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.NotNull(operations);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldCreateNewCategory()
        {
            var categoryDto = new SaveCategoryDto
            {
                Name = "Test Category"
            };

            var (createdCategory, statusCode) = await SendRequestAsync<CategoryDto>(HttpMethod.Post, ApiRoutes.Categories, categoryDto);
            Assert.Equal(HttpStatusCode.Created, statusCode);

            var (retrievedCategory, getStatusCode) = await SendRequestAsync<CategoryDto>(HttpMethod.Get, $"{ApiRoutes.Categories}{createdCategory.Id}");

            Assert.Equal(HttpStatusCode.OK, getStatusCode);
            Assert.Equal(categoryDto.Name, retrievedCategory.Name);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldUpdateExistingCategory()
        {
            var updateCategoryDto = new SaveCategoryDto
            {
                Name = "Updated Category"
            };

            var (_, statusCode) = await SendRequestAsync<CategoryDto>(HttpMethod.Put, $"{ApiRoutes.Categories}1", updateCategoryDto);
            Assert.Equal(HttpStatusCode.OK, statusCode);

            var (retrievedCategory, getStatusCode) = await SendRequestAsync<CategoryDto>(HttpMethod.Get, $"{ApiRoutes.Categories}1");
            Assert.Equal(HttpStatusCode.OK, getStatusCode);
            Assert.Equal("Updated Category", retrievedCategory.Name);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldDeleteExistingCategory()
        {
            var (_, statusCode) = await SendRequestAsync<object>(HttpMethod.Delete, $"{ApiRoutes.Categories}1");
            Assert.Equal(HttpStatusCode.OK, statusCode);

            var getCategoryResponse = await TestClient.GetAsync($"{ApiRoutes.Categories}1");
            Assert.Equal(HttpStatusCode.NotFound, getCategoryResponse.StatusCode);
        }
    }
}