using Finance.App.Domain.Models;
using Finance.App.Domain.Repositories;
using Finance.App.Domain.Services.Communication;
using Finance.App.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Finance.Tests.Services
{
    public class OperationServiceTests
    {
        private readonly Mock<IOperationRepository> _operationRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<OperationService>> _loggerMock;
        private readonly OperationService _operationService;

        public OperationServiceTests()
        {
            _operationRepositoryMock = new Mock<IOperationRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<OperationService>>();

            _operationService = new OperationService(_operationRepositoryMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCalled_ReturnsOperation()
        {
            // Arrange
            int id = 1;
            FinancialOperation expectedOperation = new FinancialOperation
            {
                Id = id,
                Name = "Test Operation",
                Amount = 100,
                Date = new DateTime(2022, 01, 01),
                IsIncome = true,
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "Test Category" }
            };

            _operationRepositoryMock.Setup(repo => repo.FindByIdAsync(id)).ReturnsAsync(expectedOperation);

            // Act
            var operation = await _operationService.GetByIdAsync(id);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal(expectedOperation.Id, operation.Id);
            Assert.Equal(expectedOperation.Name, operation.Name);
            Assert.Equal(expectedOperation.Amount, operation.Amount);
            Assert.Equal(expectedOperation.Date, operation.Date);
            Assert.Equal(expectedOperation.IsIncome, operation.IsIncome);
            Assert.Equal(expectedOperation.CategoryId, operation.CategoryId);
            Assert.Equal(expectedOperation.Category.Name, operation.Category.Name);
        }

        [Fact]
        public async Task DeleteAsync_WhenOperationExists_ShouldReturnSuccessfulResponse()
        {
            // Arrange
            var operationId = 1;
            var existingOperation = new FinancialOperation { Id = operationId };
            _operationRepositoryMock.Setup(repo => repo.FindByIdAsync(operationId))
                .ReturnsAsync(existingOperation);

            // Act
            var response = await _operationService.DeleteAsync(operationId);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Empty(response.Message);
            Assert.Equal(existingOperation, response.Operation);
            _operationRepositoryMock.Verify(repo => repo.Remove(existingOperation), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenOperationDoesNotExist_ShouldReturnNotFoundResponse()
        {
            // Arrange
            var operationId = 1;
            _operationRepositoryMock.Setup(repo => repo.FindByIdAsync(operationId))
                .ReturnsAsync(null as FinancialOperation);

            // Act
            var response = await _operationService.DeleteAsync(operationId);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Null(response.Operation);
            Assert.Equal("Operation not found.", response.Message);
            _operationRepositoryMock.Verify(repo => repo.Remove(It.IsAny<FinancialOperation>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenExceptionOccurs_ShouldReturnErrorResponse()
        {
            // Arrange
            var operationId = 1;
            var existingOperation = new FinancialOperation { Id = operationId };
            _operationRepositoryMock.Setup(repo => repo.FindByIdAsync(operationId))
                .ReturnsAsync(existingOperation);
            _operationRepositoryMock.Setup(repo => repo.Remove(existingOperation))
                .Throws(new Exception("An error occurred"));

            // Act
            var response = await _operationService.DeleteAsync(operationId);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Null(response.Operation);
            Assert.Equal("An error occurred when deleting the operation: An error occurred", response.Message);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task ListByDateAsync_WithValidStartDate_ReturnsOperations()
        {
            // Arrange
            var startDateString = "2022-01-01";
            var operations = new List<FinancialOperation>
            {
                new FinancialOperation { Id = 1, Name = "Income 1", Amount = 100, Date = new DateTime(2022, 1, 1), IsIncome = true, CategoryId = 1 },
                new FinancialOperation { Id = 2, Name = "Expense 1", Amount = 50, Date = new DateTime(2022, 1, 2), IsIncome = false, CategoryId = 2 },
                new FinancialOperation { Id = 3, Name = "Income 2", Amount = 200, Date = new DateTime(2022, 2, 1), IsIncome = true, CategoryId = 1 },
                new FinancialOperation { Id = 4, Name = "Expense 2", Amount = 75, Date = new DateTime(2022, 2, 2), IsIncome = false, CategoryId = 2 },
            };
            _operationRepositoryMock.Setup(r => r.ListByDateAsync(It.IsAny<DateTime>(), null)).ReturnsAsync(operations.Where(o => o.Date >= new DateTime(2022, 1, 1)));

            // Act
            var response = await _operationService.ListByDateAsync(startDateString);

            // Assert
            Assert.NotNull(response.Operations);
            Assert.Equal(4, response.Operations.Count());
            Assert.Equal(300, response.Income);
            Assert.Equal(125, response.Expenses);
            Assert.Empty(response.Message);
        }

        [Fact]
        public async Task ListByDateAsync_WithInvalidStartDate_ReturnsErrorMessage()
        {
            // Arrange
            var startDateString = "invalid date format";

            // Act
            var response = await _operationService.ListByDateAsync(startDateString);

            // Assert
            Assert.Null(response.Operations);
            Assert.Equal("Invalid start date format.", response.Message);
        }

        [Fact]
        public async Task ListByDateAsync_WithValidDates_ReturnsOperations()
        {
            // Arrange
            var operations = new List<FinancialOperation>
            {
                new FinancialOperation
                {
                    Name = "Salary",
                    Amount = 5000,
                    Date = new DateTime(2022, 01, 15),
                    IsIncome = true,
                    CategoryId = 1,
                    Category = new Category { Name = "Income" }
                },
                new FinancialOperation
                {
                    Name = "Rent",
                    Amount = 1000,
                    Date = new DateTime(2022, 01, 05),
                    IsIncome = false,
                    CategoryId = 2,
                    Category = new Category { Name = "Expenses" }
                },
                new FinancialOperation
                {
                    Name = "Bonus",
                    Amount = 1000,
                    Date = new DateTime(2022, 02, 01),
                    IsIncome = true,
                    CategoryId = 1,
                    Category = new Category { Name = "Income" }
                },
                new FinancialOperation
                {
                    Name = "Groceries",
                    Amount = 250,
                    Date = new DateTime(2022, 02, 05),
                    IsIncome = false,
                    CategoryId = 2,
                    Category = new Category { Name = "Expenses" }
                },
                new FinancialOperation
                {
                    Name = "Gift",
                    Amount = 100,
                    Date = new DateTime(2022, 02, 14),
                    IsIncome = true,
                    CategoryId = 1,
                    Category = new Category { Name = "Income" }
                },
            };

            _operationRepositoryMock.Setup(x => x.ListByDateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime?>()))
                          .ReturnsAsync((DateTime startDate, DateTime? endDate) =>
                          {
                              var filteredOperations = operations.Where(o => o.Date >= startDate);
                              if (endDate.HasValue)
                              {
                                  filteredOperations = filteredOperations.Where(o => o.Date <= endDate);
                              }
                              return filteredOperations;
                          });

            // Act
            var result = await _operationService.ListByDateAsync("2022-01-01", "2022-02-28");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Operations.Count());
            Assert.Equal(6100, result.Income);
            Assert.Equal(1250, result.Expenses);
        }

        [Fact]
        public async Task SaveAsync_WithValidOperation_ReturnsSuccessResponse()
        {
            // Arrange
            var operation = new FinancialOperation { Name = "Salary", Amount = 1000, Date = DateTime.Now, IsIncome = true };

            // Act
            var response = await _operationService.SaveAsync(operation);

            // Assert
            Assert.True(response.Success);
            Assert.Empty(response.Message);
            Assert.NotNull(response.Operation);
            Assert.Equal(operation.Name, response.Operation.Name);
            Assert.Equal(operation.Amount, response.Operation.Amount);
            Assert.Equal(operation.Date, response.Operation.Date);
            Assert.Equal(operation.IsIncome, response.Operation.IsIncome);
        }

        [Fact]
        public async Task SaveAsync_WithInvalidOperation_ReturnsErrorResponse()
        {
            // Arrange
            _operationRepositoryMock.Setup(r => r.AddAsync(It.IsAny<FinancialOperation>())).ThrowsAsync(new Exception());
            var operation = new FinancialOperation { Name = "Salary", Amount = 1000, Date = DateTime.Now, IsIncome = true };

            // Act
            var response = await _operationService.SaveAsync(operation);

            // Assert
            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            Assert.Null(response.Operation);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingOperation_ReturnsNotFoundResponse()
        {
            // Arrange
            int operationId = 1;
            FinancialOperation operationToUpdate = new FinancialOperation
            {
                Id = operationId,
                Name = "New Operation",
                Amount = 100,
                Date = DateTime.Now,
                IsIncome = true,
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "Salary" }
            };

            _operationRepositoryMock.Setup(repo => repo.FindByIdAsync(operationId))
                .ReturnsAsync((FinancialOperation)null);

            // Act
            var response = await _operationService.UpdateAsync(operationId, operationToUpdate);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Success == false);
            Assert.Equal("Operation not found.", response.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithExistingOperation_ReturnsUpdatedOperation()
        {
            // Arrange
            int operationId = 1;
            FinancialOperation existingOperation = new FinancialOperation
            {
                Id = operationId,
                Name = "Operation 1",
                Amount = 50,
                Date = DateTime.Now.AddDays(-1),
                IsIncome = true,
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "Salary" }
            };
            FinancialOperation operationToUpdate = new FinancialOperation
            {
                Id = operationId,
                Name = "New Operation",
                Amount = 100,
                Date = DateTime.Now,
                IsIncome = true,
                CategoryId = 2,
                Category = new Category { Id = 2, Name = "Bonus" }
            };

            _operationRepositoryMock.Setup(repo => repo.FindByIdAsync(operationId))
                .ReturnsAsync(existingOperation);

            // Act
            var response = await _operationService.UpdateAsync(operationId, operationToUpdate);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Empty(response.Message);
            Assert.Equal(operationId, response.Operation.Id);
            Assert.Equal(operationToUpdate.Name, response.Operation.Name);
            Assert.Equal(operationToUpdate.Amount, response.Operation.Amount);
            Assert.Equal(operationToUpdate.Date, response.Operation.Date);
            Assert.Equal(operationToUpdate.IsIncome, response.Operation.IsIncome);
            Assert.Equal(operationToUpdate.CategoryId, response.Operation.CategoryId);
            Assert.Equal(operationToUpdate.Category, response.Operation.Category);
        }

        [Fact]
        public async Task UpdateAsync_WithExceptionThrown_ReturnsErrorResponse()
        {
            // Arrange
            var id = 1;
            var operation = new FinancialOperation { Name = "Test Operation", Amount = 100, Date = DateTime.Now, IsIncome = true, CategoryId = 1 };

            _operationRepositoryMock.Setup(r => r.FindByIdAsync(id))
                .ReturnsAsync(new FinancialOperation { Id = id, Name = "Existing Operation", Amount = 200, Date = DateTime.Now, IsIncome = false, CategoryId = 2 });

            _operationRepositoryMock.Setup(r => r.Update(It.IsAny<FinancialOperation>()))
                .Throws(new Exception("An error occurred while updating the operation"));

            // Act
            var response = await _operationService.UpdateAsync(id, operation);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("An error occurred when updating the operation: An error occurred while updating the operation", response.Message);
            Assert.Null(response.Operation);
        }
    }
}
