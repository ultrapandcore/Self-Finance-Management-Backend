using Finance.App.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Finance.App.Persistence.Contexts
{
    public class AppDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<FinancialOperation> Operations { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>().ToTable("Categories");
            builder.Entity<Category>().HasKey(c => c.Id);
            builder.Entity<Category>().Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<Category>().Property(c => c.Name).IsRequired().HasMaxLength(30);
            builder.Entity<Category>().Property(c => c.Deleted).HasDefaultValue(false);
            builder.Entity<Category>().HasMany(c => c.Operations).WithOne(fo => fo.Category).HasForeignKey(fo => fo.CategoryId);
            builder.Entity<Category>().HasQueryFilter(c => !c.Deleted);

            builder.Entity<FinancialOperation>().ToTable("Operations");
            builder.Entity<FinancialOperation>().HasKey(fo => fo.Id);
            builder.Entity<FinancialOperation>().Property(fo => fo.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<FinancialOperation>().Property(fo => fo.Name).IsRequired().HasMaxLength(50);
            builder.Entity<FinancialOperation>().Property(fo => fo.Deleted).HasDefaultValue(false);
            builder.Entity<FinancialOperation>().HasQueryFilter(fo => !fo.Deleted);
        }
    }
}
