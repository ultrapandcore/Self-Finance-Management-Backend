using Finance.App.Domain.Models;
using Finance.App.Domain.Services.Communication;

namespace Finance.App.Domain.Repositories
{
    public interface IOperationRepository
    {
        Task<IEnumerable<FinancialOperation>> ListAsync();
        Task<IEnumerable<FinancialOperation>> ListByDateAsync(DateTime startDate, DateTime? endDate);
        Task AddAsync(FinancialOperation category);
        Task<FinancialOperation> FindByIdAsync(int id);
        void Update(FinancialOperation category);
        void Remove(FinancialOperation category);
    }
}
