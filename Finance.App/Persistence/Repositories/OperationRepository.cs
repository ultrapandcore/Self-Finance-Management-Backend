using Finance.App.Domain.Models;
using Finance.App.Domain.Repositories;
using Finance.App.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Finance.App.Persistence.Repositories
{
    public class OperationRepository : BaseRepository, IOperationRepository
    {
        public OperationRepository(AppDbContext context) : base(context) { }

        public async Task AddAsync(FinancialOperation operation)
        {
            await _context.Operations.AddAsync(operation);
        }

        public async Task<FinancialOperation> FindByIdAsync(int id)
        {
            return await _context.Operations.FindAsync(id);
        }

        public async Task<IEnumerable<FinancialOperation>> ListAsync()
        {
            return await _context.Operations.ToListAsync();
        }

        public async Task<IEnumerable<FinancialOperation>> ListByDateAsync(DateTime startDate, DateTime? endDate)
        {
            if (endDate == null)
            {
                return await _context.Operations.Where(o => o.Date.Date == startDate.Date).ToListAsync();
            }

            return await _context.Operations.Where(o => o.Date.Date >= startDate.Date && o.Date.Date <= endDate.Value.Date).ToListAsync();
        }

        public void Remove(FinancialOperation operation)
        {
            operation.Deleted = true;
            _context.Operations.Update(operation);
        }

        public void Update(FinancialOperation operation)
        {
            _context.Operations.Update(operation);
        }
    }
}
