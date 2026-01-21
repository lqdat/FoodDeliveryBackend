using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FoodDeliveryBackend.Infrastructure.Data;

public class FoodDeliveryDbContextFactory : IDesignTimeDbContextFactory<FoodDeliveryDbContext>
{
    public FoodDeliveryDbContext CreateDbContext(string[] args)
    {
        // Try to load .env from common locations
        DotNetEnv.Env.Load(); 
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_HOST")))
        {
             // Fallback: Try loading from API folder if running from root
             DotNetEnv.Env.Load("FoodDeliveryBackend.API/.env");
        }

        var optionsBuilder = new DbContextOptionsBuilder<FoodDeliveryDbContext>();
        
        var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};Port={Environment.GetEnvironmentVariable("DB_PORT")};Database={Environment.GetEnvironmentVariable("DB_NAME")};Username={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";
        
        optionsBuilder.UseNpgsql(connectionString);

        return new FoodDeliveryDbContext(optionsBuilder.Options);
    }
}
