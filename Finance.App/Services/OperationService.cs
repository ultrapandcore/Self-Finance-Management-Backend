using Finance.App.Domain.Models;
using Finance.App.Domain.Repositories;
using Finance.App.Domain.Services;
using Finance.App.Domain.Services.Communication;

namespace Finance.App.Services
{
    public class OperationService : IOperationService
    {
        private readonly IOperationRepository _operationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OperationService> _logger;

        public OperationService(IOperationRepository operationRepository, IUnitOfWork unitOfWork, ILogger<OperationService> logger)
        {
            _operationRepository = operationRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<OperationResponse> DeleteAsync(int id)
        {
            var existingOperation = await _operationRepository.FindByIdAsync(id);

            if (existingOperation == null)
            {
                return new OperationResponse("Operation not found.");
            }

            try
            {
                _operationRepository.Remove(existingOperation);
                await _unitOfWork.CompleteAsync();

                return new OperationResponse(existingOperation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred when deleting the operation with ID {id}");
                return new OperationResponse($"An error occurred when deleting the operation: {ex.Message}");
            }
        }

        public async Task<FinancialOperation> GetByIdAsync(int id)
        {
            return await _operationRepository.FindByIdAsync(id);
        }

        public async Task<IEnumerable<FinancialOperation>> ListAsync()
        {
            return await _operationRepository.ListAsync();
        }

        public async Task<DateOperationResponse> ListByDateAsync(string startDateString, string endDateString = null)
        {
            if (!DateTime.TryParse(startDateString, out DateTime startDate))
            {
                return new DateOperationResponse("Invalid start date format.");
            }

            DateTime? endDate = null;
            if (!string.IsNullOrEmpty(endDateString))
            {
                if (!DateTime.TryParse(endDateString, out var parsedEndDate))
                {
                    return new DateOperationResponse("Invalid end date format.");
                }
                endDate = parsedEndDate;
            }

            var operations = await _operationRepository.ListByDateAsync(startDate, endDate);

            var income = operations.Where(o => o.IsIncome).Sum(o => o.Amount);
            var expenses = operations.Where(o => !o.IsIncome).Sum(o => o.Amount);

            return new DateOperationResponse(operations, income, expenses);
        }

        public async Task<OperationResponse> SaveAsync(FinancialOperation operation)
        {
            try
            {
                await _operationRepository.AddAsync(operation);
                await _unitOfWork.CompleteAsync();

                return new OperationResponse(operation);
            }
            catch (Exception ex)
            {
                return new OperationResponse($"An error occurred when saving the operation: {ex.Message}");
            }
        }

        public async Task<OperationResponse> UpdateAsync(int id, FinancialOperation operation)
        {
            var existingOperation = await _operationRepository.FindByIdAsync(id);

            if (existingOperation == null)
            {
                return new OperationResponse("Operation not found.");
            }

            existingOperation.Name = operation.Name;
            existingOperation.Amount = operation.Amount;
            existingOperation.Date = operation.Date;
            existingOperation.IsIncome = operation.IsIncome;
            existingOperation.CategoryId = operation.CategoryId;
            existingOperation.Category = operation.Category;

            try
            {
                _operationRepository.Update(existingOperation);
                await _unitOfWork.CompleteAsync();

                return new OperationResponse(existingOperation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred when updating the operation with ID {id}");
                return new OperationResponse($"An error occurred when updating the operation: {ex.Message}");
            }
        }
    }
}
