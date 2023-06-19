using Finance.App.Domain.Repositories;
using Finance.App.Domain.Services;
using Finance.App.Persistence.Contexts;
using Finance.App.Persistence.Repositories;
using Finance.App.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

public class Program
{
    private static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();

        string connectionString = builder.Configuration.GetConnectionString("FinanceAppContext");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Finance API", Version = "v1" });

            options.ResolveConflictingActions(x => x.First());

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                BearerFormat = "JWT",
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri($"https://{configuration["Auth0:Domain"]}/oauth/token"),
                        AuthorizationUrl = new Uri($"https://{configuration["Auth0:Domain"]}/authorize?audience={configuration["Auth0:Audience"]}"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenId" },
                        }
                    }
                }
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    new[] { "openid" }
                }
            });
        });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = $"https://{configuration["Auth0:Domain"]}/";
            options.Audience = configuration["Auth0:Audience"];
        });

        builder.Services.AddScoped<IOperationRepository, OperationRepository>();
        builder.Services.AddScoped<IOperationService, OperationService>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(settings =>
            {
                settings.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1.0");
                settings.OAuthClientId(configuration["Auth0:ClientId"]);
                settings.OAuthClientSecret(configuration["Auth0:ClientSecret"]);
                settings.OAuthUsePkce();
            });
        }

        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}