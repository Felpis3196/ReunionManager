using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartMeetingManager.Application.Interfaces;
using SmartMeetingManager.Domain.Interfaces;
using SmartMeetingManager.Infrastructure.Data;
using SmartMeetingManager.Infrastructure.Repositories;
using SmartMeetingManager.Infrastructure.Services;
using SmartMeetingManager.Application.UseCases.Meetings;
using SmartMeetingManager.API.Hubs;
using System.Reflection;
using System.Linq;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Disable automatic 400 responses to handle validation manually
        options.SuppressModelStateInvalidFilter = false;
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return new BadRequestObjectResult(new
            {
                error = "Dados inválidos",
                message = string.Join("; ", errors),
                details = errors,
                fields = context.ModelState.Keys.ToList()
            });
        };
    });

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
    {
        npgsqlOptions.MigrationsAssembly("SmartMeetingManager.Infrastructure");
    });
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SmartMeetingManager_DefaultSecretKey_ChangeInProduction_12345678";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SmartMeetingManager";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SmartMeetingManagerApp";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers["Token-Expired"] = "true";
            }
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var path = context.Request.Path;
            if (path.StartsWithSegments("/hubs/teamchat", StringComparison.OrdinalIgnoreCase))
            {
                var token = context.Request.Query["access_token"].FirstOrDefault();
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrganizationPermissionService, OrganizationPermissionService>();
builder.Services.AddScoped<IOrganizationRoleService, OrganizationRoleService>();
builder.Services.AddScoped<ITeamChatService, TeamChatService>();

builder.Services.AddSignalR();

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
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
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
            
            // Apply pending migrations in Development (so you don't need psql/dotnet-ef)
            if (app.Environment.IsDevelopment())
            {
                var pending = await dbContext.Database.GetPendingMigrationsAsync();
                if (pending.Any())
                {
                    logger.LogInformation("Applying pending migrations: {Migrations}", string.Join(", ", pending));
                    await dbContext.Database.MigrateAsync();
                }
            }
            
            // Verify that tables exist - if not, use EnsureCreated as fallback
            logger.LogInformation("Verifying database schema...");
            bool tablesExist = false;
            try
            {
                var result = await dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Users'");
                tablesExist = result > 0;
                logger.LogInformation($"Users table exists: {tablesExist}");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Could not verify Users table: {ex.Message}");
            }
            
            // If tables don't exist, create them using EnsureCreated
            if (!tablesExist)
            {
                logger.LogWarning("Tables do not exist. Creating database schema using EnsureCreated...");
                await dbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("Database schema created successfully.");
            }
            
            // Seed data de exemplo
            logger.LogInformation("Seeding development sample data (if database is empty)...");
            await SmartMeetingManager.Infrastructure.Data.SeedData.SeedAsync(dbContext);
            logger.LogInformation("Sample data seed completed.");

            // Garantir existencia de um SiteAdmin default e organizacao global
            logger.LogInformation("Ensuring default SiteAdmin user and global organization...");
            await SmartMeetingManager.Infrastructure.Data.SeedData.EnsureSiteAdminAsync(dbContext, logger, configuration);
            logger.LogInformation("EnsureSiteAdminAsync completed.");
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<TeamChatHub>("/hubs/teamchat");

app.Run();