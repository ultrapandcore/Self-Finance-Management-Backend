using Finance.App.Persistence.Contexts;
using GST.Fake.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Finance.IntegrationTests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly SqliteConnection _sqliteConnection;

        public CustomWebApplicationFactory()
        {
            _sqliteConnection = new SqliteConnection("DataSource=:memory:");
            _sqliteConnection.Open();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _sqliteConnection.Close();
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = FakeJwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = FakeJwtBearerDefaults.AuthenticationScheme;
                }).AddFakeJwtBearer();

                services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)));
                services.AddDbContext<AppDbContext>(options => options.UseSqlite(_sqliteConnection));

                var serviceProvider = services.BuildServiceProvider();

                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Set Ukrainian culture
                    var ukCulture = new CultureInfo("uk-UA");
                    CultureInfo.CurrentCulture = ukCulture;
                    CultureInfo.CurrentUICulture = ukCulture;

                    SeedData.ApplyMigrationsAndSeedData(dbContext, scope.ServiceProvider).Wait();
                }
            });
        }
    }
}

