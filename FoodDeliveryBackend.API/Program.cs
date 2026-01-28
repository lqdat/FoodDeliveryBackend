using FoodDeliveryBackend.Infrastructure.Data;
using FoodDeliveryBackend.API.Services;
using FoodDeliveryBackend.Core.Entities.Admin;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load(); // Load .env file

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Database
var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};Port={Environment.GetEnvironmentVariable("DB_PORT")};Database={Environment.GetEnvironmentVariable("DB_NAME")};Username={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";

builder.Services.AddDbContext<FoodDeliveryDbContext>(options =>
    options.UseNpgsql(connectionString)
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// 2. CORS (Allow React Native access)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 3. JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "super_secret_key_change_this_in_production_1234567890";
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// 4. Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Admin Role Policies
    options.AddPolicy("SuperAdminOnly", policy =>
        policy.RequireRole(AdminRole.SuperAdmin.ToString()));
    
    options.AddPolicy("MasterAdminOnly", policy =>
        policy.RequireRole(AdminRole.AdminRestaurantMaster.ToString()));
    
    options.AddPolicy("RegionAdminOnly", policy =>
        policy.RequireRole(AdminRole.AdminRestaurantRegion.ToString()));
    
    options.AddPolicy("CanApprove", policy =>
        policy.RequireRole(
            AdminRole.AdminRestaurantMaster.ToString(),
            AdminRole.AdminRestaurantRegion.ToString()));
    
    // ChainOwner Policies
    options.AddPolicy("ChainOwnerOnly", policy =>
        policy.RequireClaim("AccountType", "ChainOwner"));
    
    options.AddPolicy("StoreManagerOnly", policy =>
        policy.RequireClaim("AccountType", "StoreManager"));
    
    options.AddPolicy("ChainOwnerOrManager", policy =>
        policy.RequireClaim("AccountType", "ChainOwner", "StoreManager"));
});

// 5. Application Services
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IFoodService, FoodService>();

builder.Services.AddSignalR();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "FoodDeliveryBackend API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization using the Bearer scheme. Enter your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    // Auto-seed data in Development (unless SKIP_SEEDING=true)
    var skipSeeding = Environment.GetEnvironmentVariable("SKIP_SEEDING")?.ToLower() == "true";
    
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<FoodDeliveryDbContext>();
        try
        {
            dbContext.Database.Migrate(); 
            
            if (!skipSeeding)
            {
                await DbSeeder.SeedAsync(dbContext);
                
                // Seed Admin accounts
                await AdminSeeder.SeedAdminAccountsAsync(dbContext);
            }
            else
            {
                Console.WriteLine("Skipping database seeding (SKIP_SEEDING=true)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding data: {ex.Message}");
        }
    }
}


app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles(); // Enable serving files from wwwroot

// Serve files from Railway Volume /data if it exists
if (Directory.Exists("/data"))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider("/data"),
        RequestPath = ""
    });
}

app.UseAuthentication(); // Enable Auth
app.UseAuthorization();

app.MapControllers();
app.MapHub<FoodDeliveryBackend.API.Hubs.NotificationHub>("/hubs/notifications");
app.MapHub<FoodDeliveryBackend.API.Hubs.ChatHub>("/hubs/chat");

// Redirect root to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));


// Standard Run for Production/Docker (respects ASPNETCORE_URLS)
app.Run();
