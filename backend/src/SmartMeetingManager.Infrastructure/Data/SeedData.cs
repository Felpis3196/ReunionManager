using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartMeetingManager.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace SmartMeetingManager.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        try
        {
            // Skip seeding if data already exists
            if (await context.Users.AnyAsync())
                return;
        }
        catch (Exception ex)
        {
            // If tables don't exist, throw to be caught by caller
            // This indicates migrations weren't applied correctly
            throw new InvalidOperationException(
                "Cannot seed database: tables do not exist. Ensure migrations are applied first.", ex);
        }

        // Use fixed GUIDs for test data (for consistency)
        var organizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var userId1 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var userId2 = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        // Create test organization
        var organization = new Organization
        {
            Id = organizationId,
            Name = "Empresa Teste",
            Description = "Organização de teste",
            CreatedAt = DateTime.UtcNow
        };
        await context.Organizations.AddAsync(organization);

        // Create test users (password: Test@123)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test@123", workFactor: 10);
        var user1 = new User
        {
            Id = userId1,
            Email = "admin@test.com",
            Name = "Admin User",
            PasswordHash = passwordHash,
            IsActive = true,
            IsSiteAdmin = false, // Site Admin é criado por EnsureSiteAdminAsync (admin@smm.local)
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new User
        {
            Id = userId2,
            Email = "user@test.com",
            Name = "Regular User",
            PasswordHash = passwordHash,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddRangeAsync(user1, user2);

        // Add users to organization
        var member1 = new OrganizationMember
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            OrganizationId = organization.Id,
            Role = OrganizationRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        var member2 = new OrganizationMember
        {
            Id = Guid.NewGuid(),
            UserId = user2.Id,
            OrganizationId = organization.Id,
            Role = OrganizationRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        await context.OrganizationMembers.AddRangeAsync(member1, member2);

        // Create test project
        var project = new Project
        {
            Id = projectId,
            OrganizationId = organization.Id,
            Name = "Projeto Exemplo",
            Description = "Projeto de exemplo para testes",
            CreatedAt = DateTime.UtcNow
        };
        await context.Projects.AddAsync(project);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Garante a existencia de um usuario Site Admin default e, opcionalmente, de uma organizacao global.
    /// Idempotente: so cria quando ainda nao existe nenhum SiteAdmin.
    /// </summary>
    public static async Task EnsureSiteAdminAsync(
        ApplicationDbContext context,
        ILogger logger,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        const string defaultAdminEmail = "admin@smm.local";
        const string defaultOrgName = "SmartMeeting Global";
        const string defaultPassword = "Admin#123";

        // Se ja existe qualquer SiteAdmin, nao cria outro por padrao
        var anySiteAdmin = await context.Users.AnyAsync(u => u.IsSiteAdmin, cancellationToken);
        if (anySiteAdmin)
        {
            logger.LogInformation("At least one SiteAdmin user already exists. Skipping default admin seed.");
            return;
        }

        logger.LogInformation("No SiteAdmin found. Creating default SiteAdmin user {Email}.", defaultAdminEmail);

        // Escolher senha: configuracao override ou senha random
        var configuredPassword = defaultPassword;
        string generatedPassword;
        if (!string.IsNullOrWhiteSpace(configuredPassword))
        {
            generatedPassword = configuredPassword;
            logger.LogInformation("Using Seed:Admin:Password from configuration for default admin user.");
        }
        else
        {
            generatedPassword = GenerateStrongPassword();
            logger.LogInformation("Generated random password for default admin user.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(generatedPassword, workFactor: 12);

        var existingAdminUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == defaultAdminEmail.ToLower(), cancellationToken);

        if (existingAdminUser == null)
        {
            existingAdminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = defaultAdminEmail,
                Name = "Admin",
                PasswordHash = passwordHash,
                IsActive = true,
                IsSiteAdmin = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(existingAdminUser, cancellationToken);
            logger.LogInformation("Default SiteAdmin user {Email} created.", defaultAdminEmail);
        }
        else
        {
            existingAdminUser.IsSiteAdmin = true;
            existingAdminUser.IsActive = true;
            if (string.IsNullOrEmpty(existingAdminUser.PasswordHash))
            {
                existingAdminUser.PasswordHash = passwordHash;
            }
            logger.LogInformation("Existing user {Email} promoted to SiteAdmin.", defaultAdminEmail);
        }

        // Criar organizacao global se necessario
        var globalOrg = await context.Organizations
            .FirstOrDefaultAsync(o => o.Name == defaultOrgName, cancellationToken);

        if (globalOrg == null)
        {
            globalOrg = new Organization
            {
                Id = Guid.NewGuid(),
                Name = defaultOrgName,
                CreatedAt = DateTime.UtcNow
            };
            await context.Organizations.AddAsync(globalOrg, cancellationToken);
            logger.LogInformation("Global organization {Name} created.", defaultOrgName);
        }

        // Garantir membership do admin como Owner nessa organizacao
        var hasMembership = await context.OrganizationMembers.AnyAsync(
            m => m.OrganizationId == globalOrg.Id && m.UserId == existingAdminUser.Id,
            cancellationToken);

        if (!hasMembership)
        {
            var membership = new OrganizationMember
            {
                Id = Guid.NewGuid(),
                OrganizationId = globalOrg.Id,
                UserId = existingAdminUser.Id,
                Role = OrganizationRole.Owner,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };
            await context.OrganizationMembers.AddAsync(membership, cancellationToken);
            logger.LogInformation("Default SiteAdmin user added as Owner of organization {Name}.", defaultOrgName);
        }

        await context.SaveChangesAsync(cancellationToken);

        // Logar a senha apenas uma vez (cuidado em producao)
        logger.LogWarning(
            "Default SiteAdmin credentials - Email: {Email} Password: {Password}. " +
            "Change this password after first login.",
            defaultAdminEmail, defaultPassword);
    }

    private static string GenerateStrongPassword(int length = 16)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*()-_=+";
        var bytes = new byte[length];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var resultChars = new char[length];
        for (int i = 0; i < length; i++)
        {
            resultChars[i] = chars[bytes[i] % chars.Length];
        }
        return new string(resultChars);
    }
}
