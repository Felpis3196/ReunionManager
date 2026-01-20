using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartMeetingManager.Infrastructure.Data;

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        { "ConnectionStrings:DefaultConnection", Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
            ?? "Host=postgres;Database=SmartMeetingManager;Username=postgres;Password=postgres" }
    })
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? "Host=postgres;Database=SmartMeetingManager;Username=postgres;Password=postgres";

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
{
    npgsqlOptions.MigrationsAssembly("SmartMeetingManager.Infrastructure");
});

using var context = new ApplicationDbContext(optionsBuilder.Options);
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
var logger = loggerFactory.CreateLogger("Migrations");

try
{
    logger.LogInformation("Checking database connection...");
    var canConnect = await context.Database.CanConnectAsync();
    if (!canConnect)
    {
        logger.LogError("Cannot connect to database.");
        Environment.Exit(1);
    }

    logger.LogInformation("Getting pending migrations...");
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    var pendingList = pendingMigrations.ToList();
    
    if (pendingList.Count == 0)
    {
        logger.LogInformation("No pending migrations found.");
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        var appliedList = appliedMigrations.ToList();
        
        if (appliedList.Count == 0)
        {
            logger.LogWarning("No migrations found and none applied. Checking if database schema exists...");
            var tablesExist = false;
            try
            {
                var result = await context.Database.ExecuteSqlRawAsync(
                    "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Users'");
                tablesExist = result > 0;
            }
            catch
            {
                tablesExist = false;
            }
            
            if (!tablesExist)
            {
                logger.LogWarning("Database schema does not exist. Creating schema using EnsureCreated...");
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database schema created successfully using EnsureCreated.");
            }
            else
            {
                logger.LogInformation("Database schema already exists.");
            }
        }
        else
        {
            logger.LogInformation($"Applied migrations: {string.Join(", ", appliedList)}");
        }
        Environment.Exit(0);
    }

    logger.LogInformation($"Found {pendingList.Count} pending migration(s): {string.Join(", ", pendingList)}");
    
    logger.LogInformation("Applying migrations...");
    await context.Database.MigrateAsync();
    
    logger.LogInformation("Migrations applied successfully!");
    
    var finalAppliedMigrations = await context.Database.GetAppliedMigrationsAsync();
    logger.LogInformation($"All applied migrations: {string.Join(", ", finalAppliedMigrations)}");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error applying migrations");
    Environment.Exit(1);
}
