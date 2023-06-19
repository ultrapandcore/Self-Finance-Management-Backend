using Finance.App.Domain.Models;
using Finance.App.Domain.Services.Communication;

namespace Finance.App.Domain.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> ListAsync();
        Task<IEnumerable<FinancialOperation>> GetOperationsByCategoryId(int id);
        Task<Category> GetByIdAsync(int id);
        Task<CategoryResponse> SaveAsync(Category category);
        Task<CategoryResponse> UpdateAsync(int id, Category category);
        Task<CategoryResponse> DeleteAsync(int id);

    }
}
