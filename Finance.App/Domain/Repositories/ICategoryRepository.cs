using Finance.App.Domain.Models;

namespace Finance.App.Domain.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> ListAsync();
        Task<IEnumerable<FinancialOperation>> GetOperationsByCategoryId(int categoryId);
        Task AddAsync(Category category);
        Task<Category> FindByIdAsync(int id);
        Task<Category> FindByNameAsync(string name);
        void Update(Category category);
        void Remove(Category category);
    }
}
