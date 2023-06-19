using Finance.App.Domain.Models;
using Finance.App.Domain.Repositories;
using Finance.App.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Finance.App.Persistence.Repositories
{
    public class CategoryRepository : BaseRepository, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context) { }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public async Task<Category> FindByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category> FindByNameAsync(string name)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<IEnumerable<FinancialOperation>> GetOperationsByCategoryId(int categoryId)
        {
            return await _context.Operations.Where(o => o.CategoryId == categoryId).ToListAsync();
        }

        public async Task<IEnumerable<Category>> ListAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public void Remove(Category category)
        {
            category.Deleted = true;
            _context.Categories.Update(category);

            var operations = _context.Operations.Where(o => o.CategoryId == category.Id);
            foreach (var operation in operations)
            {
                operation.Deleted = true;
                _context.Operations.Update(operation);
            }
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
        }
    }
}
