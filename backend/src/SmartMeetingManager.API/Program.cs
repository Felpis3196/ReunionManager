using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SmartMeetingManager.Domain.Interfaces;
using SmartMeetingManager.Infrastructure.Data;
using SmartMeetingManager.Infrastructure.Repositories;
using SmartMeetingManager.Infrastructure.Services;
using SmartMeetingManager.Application.UseCases.Meetings;
using System.Reflection;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Smart Meeting Manager API",
        Version = "v1",
        Description = "API para gestão inteligente de reuniões com IA. Sistema que organiza, otimiza e extrai valor real das reuniões.",
        Contact = new OpenApiContact
        {
            Name = "Smart Meeting Manager",
            Email = "support@smartmeetingmanager.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add JWT Bearer token support (for future authentication)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Configure enum to be displayed as strings
    c.UseInlineDefinitionsForEnums();
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=SmartMeetingManager;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.MigrationsAssembly("SmartMeetingManager.Infrastructure"));
});

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAiService, AiService>();

// Use Cases
builder.Services.AddScoped<CreateMeetingCommand>();
builder.Services.AddScoped<GetMeetingByIdQuery>();
builder.Services.AddScoped<GetAllMeetingsQuery>();
builder.Services.AddScoped<UpdateMeetingCommand>();
builder.Services.AddScoped<GenerateAgendaCommand>();
builder.Services.AddScoped<ProcessTranscriptCommand>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Seed database (migrations are applied by the migrations container)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("Checking database connection...");
            var canConnect = await dbContext.Database.CanConnectAsync();
            logger.LogInformation($"Database can connect: {canConnect}");
            
            if (!canConnect)
            {
                logger.LogWarning("Cannot connect to database. Skipping seed.");
                return;
            }
            
            // Verify that tables exist
            logger.LogInformation("Verifying database schema...");
            try
            {
                var tableCheck = await dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'Users'");
                logger.LogInformation($"Users table exists check: {tableCheck}");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Could not verify Users table: {ex.Message}");
            }
            
            // Seed data (migrations should already be applied by migrations container)
            logger.LogInformation("Seeding database...");
            await SmartMeetingManager.Infrastructure.Data.SeedData.SeedAsync(dbContext);
            logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            logger.LogError($"Exception type: {ex.GetType().Name}");
            logger.LogError($"Exception message: {ex.Message}");
            if (ex.InnerException != null)
            {
                logger.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            // Don't throw - allow app to start even if seeding fails
        }
    }
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Meeting Manager API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Smart Meeting Manager API";
    c.DefaultModelsExpandDepth(-1);
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
    c.EnableValidator();
});

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();