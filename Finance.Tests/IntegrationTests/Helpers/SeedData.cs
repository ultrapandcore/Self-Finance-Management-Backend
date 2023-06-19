using Finance.App.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Finance.App.Persistence.Contexts
{
    public static class SeedData
    {
        public static async Task ApplyMigrationsAndSeedData(AppDbContext dbContext, IServiceProvider serviceProvider)
        {
            dbContext.Database.Migrate();

            await Initialize(serviceProvider);
        }

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var dbContext = new AppDbContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<AppDbContext>>()))
            {
                if (!dbContext.Categories.Any())
                {
                    var category1 = new Category
                    {
                        Id = 1,
                        Name = "Shopping"
                    };

                    var category2 = new Category
                    {
                        Id = 2,
                        Name = "Banking"
                    };

                    var category3 = new Category
                    {
                        Id = 3,
                        Name = "Taxi"
                    };

                    var category4 = new Category
                    {
                        Id = 4,
                        Name = "Services"
                    };

                    dbContext.Categories.AddRange(category1, category2, category3, category4);
                    await dbContext.SaveChangesAsync();

                    if (!dbContext.Operations.Any())
                    {
                        var operation1 = new FinancialOperation
                        {
                            Name = "Buying a jacket",
                            Amount = 2000,
                            Date = DateTime.Parse("23.02.2023"),
                            IsIncome = false,
                            Category = category1
                        };

                        var operation2 = new FinancialOperation
                        {
                            Name = "Bank transfer",
                            Amount = 100500,
                            Date = DateTime.Parse("21.02.2023"),
                            IsIncome = true,
                            Category = category2
                        };

                        var operation3 = new FinancialOperation
                        {
                            Name = "Taxi ride",
                            Amount = 150,
                            Date = DateTime.Parse("23.02.2023"),
                            IsIncome = false,
                            Category = category3
                        };

                        var operation4 = new FinancialOperation
                        {
                            Name = "Netflix subscription",
                            Amount = 20,
                            Date = DateTime.Parse("28.02.2023"),
                            IsIncome = false,
                            Category = category4
                        };

                        dbContext.Operations.AddRange(operation1, operation2, operation3, operation4);

                        category1.Operations = new List<FinancialOperation> { operation1 };
                        category2.Operations = new List<FinancialOperation> { operation2 };
                        category3.Operations = new List<FinancialOperation> { operation3 };
                        category4.Operations = new List<FinancialOperation> { operation4 };

                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
