using Finance.App.Dtos;
using Finance.App.Helpers;
using System.Net;
using Xunit;

namespace Finance.IntegrationTests.Controllers
{
    public class OperationsControllerTests : BaseControllerTests
    {
        [Fact]
        public async Task GetAllAsync_Should_Return_NonEmpty_List_Of_Financial_Operations()
        {
            var (operations, getStatusCode) = await SendRequestAsync<List<FinancialOperationDto>>(HttpMethod.Get, ApiRoutes.Operations);
            Assert.Equal(HttpStatusCode.OK, getStatusCode);
            Assert.NotEmpty(operations);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Financial_Operation_With_Specified_Id()
        {
            var (operation, statusCode) = await SendRequestAsync<FinancialOperationDto>(HttpMethod.Get, $"{ApiRoutes.Operations}1");

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.NotNull(operation);
            Assert.Equal(1, operation.Id);
        }

        [Fact]
        public async Task PostAsync_Should_Create_New_Financial_Operation()
        {
            var operationDto = new SaveFinancialOperationDto
            {
                Name = "Test operation",
                Amount = 100.00M,
                Date = DateTime.UtcNow.Date,
                CategoryId = 1
            };

            var (createdOperation, statusCode) = await SendRequestAsync<FinancialOperationDto>(HttpMethod.Post, ApiRoutes.Operations, operationDto);
            Assert.Equal(HttpStatusCode.Created, statusCode);

            var (retrievedOperation, getStatusCode) = await SendRequestAsync<FinancialOperationDto>(HttpMethod.Get, $"{ApiRoutes.Operations}{createdOperation.Id}");

            Assert.Equal(HttpStatusCode.OK, getStatusCode);
            Assert.Equal(operationDto.Name, retrievedOperation.Name);
            Assert.Equal(operationDto.Amount, retrievedOperation.Amount);
            Assert.Equal(operationDto.Date, retrievedOperation.Date);
            Assert.Equal(operationDto.CategoryId, retrievedOperation.CategoryId);
        }

        [Fact]
        public async Task PutAsync_Should_Update_Existing_Financial_Operations()
        {
            var updateDto = new SaveFinancialOperationDto
            {
                Name = "Updated operation",
                Amount = 200.00M,
                Date = DateTime.UtcNow.Date.AddDays(-1),
                CategoryId = 2
            };

            var (_, updateStatusCode) = await SendRequestAsync<FinancialOperationDto>(HttpMethod.Put, $"{ApiRoutes.Operations}1", updateDto);
            Assert.Equal(HttpStatusCode.OK, updateStatusCode);

            var (retrievedOperation, getStatusCode) = await SendRequestAsync<FinancialOperationDto>(HttpMethod.Get, $"{ApiRoutes.Operations}1");

            Assert.Equal(HttpStatusCode.OK, getStatusCode);
            Assert.NotNull(retrievedOperation);
            Assert.Equal(updateDto.Name, retrievedOperation.Name);
            Assert.Equal(updateDto.Amount, retrievedOperation.Amount);
            Assert.Equal(updateDto.Date.Date, retrievedOperation.Date.Date);
            Assert.Equal(updateDto.CategoryId, retrievedOperation.CategoryId);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Existing_Financial_Operation()
        {
            var deleteUrl = $"{ApiRoutes.Operations}1";
            var (deleteResult, deleteStatusCode) = await SendRequestAsync<FinancialOperationDto>(HttpMethod.Delete, deleteUrl);
            Assert.Equal(HttpStatusCode.OK, deleteStatusCode);
            Assert.NotNull(deleteResult);
            Assert.Equal(1, deleteResult.Id);

            var getRequest = new HttpRequestMessage(HttpMethod.Get, deleteUrl);
            var getResponse = await TestClient.SendAsync(getRequest);

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task GetDailyAsync_Should_Return_List_Of_Financial_Operations_For_Specified_Date()
        {
            var dailyUrl = $"{ApiRoutes.OperationsDaily}02.23.2023";
            var (result, statusCode) = await SendRequestAsync<OperationsByDateDto>(HttpMethod.Get, dailyUrl);
            Assert.Equal(HttpStatusCode.OK, statusCode);

            Assert.NotNull(result);
            Assert.NotEmpty(result.Operations);
        }

        [Fact]
        public async Task GetByDateRangeAsync_Should_Return_List_Of_Financial_Operations_For_Specified_Date_Range()
        {
            var rangeUrl = $"{ApiRoutes.OperationsByDateRange}02.23.2023/02.28.2023";
            var (result, statusCode) = await SendRequestAsync<OperationsByDateDto>(HttpMethod.Get, rangeUrl);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.NotNull(result);
            Assert.NotEmpty(result.Operations);
        }
    }
}