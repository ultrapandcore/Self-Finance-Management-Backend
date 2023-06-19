using Finance.App.Domain.Models;
using Finance.App.Domain.Services.Communication;

namespace Finance.App.Domain.Services
{
    public interface IOperationService
    {
        Task<IEnumerable<FinancialOperation>> ListAsync();
        Task<FinancialOperation> GetByIdAsync(int id);
        Task<DateOperationResponse> ListByDateAsync(string startDate, string endDate);
        Task<OperationResponse> SaveAsync(FinancialOperation operation);
        Task<OperationResponse> UpdateAsync(int id, FinancialOperation operation);
        Task<OperationResponse> DeleteAsync(int id);

    }
}
