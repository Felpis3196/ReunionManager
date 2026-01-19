using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Domain.Entities;

namespace SmartMeetingManager.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Skip seeding if data already exists
        if (await context.Users.AnyAsync())
            return;

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

        // Create test users
        var user1 = new User
        {
            Id = userId1,
            Email = "admin@test.com",
            Name = "Admin User",
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new User
        {
            Id = userId2,
            Email = "user@test.com",
            Name = "Regular User",
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
}
