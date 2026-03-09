using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Application.DTOs;
using SmartMeetingManager.Application.Interfaces;
using SmartMeetingManager.Domain;
using SmartMeetingManager.Domain.Entities;
using SmartMeetingManager.Infrastructure.Data;

namespace SmartMeetingManager.Infrastructure.Services;

public class OrganizationRoleService : IOrganizationRoleService
{
    private readonly ApplicationDbContext _context;

    public OrganizationRoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrganizationRoleDto>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var roles = await _context.OrganizationCustomRoles
            .Where(r => r.OrganizationId == organizationId)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
        return roles.Select(MapToDto).ToList();
    }

    public async Task<OrganizationRoleDto?> GetByIdAsync(Guid id, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var role = await _context.OrganizationCustomRoles
            .FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == organizationId, cancellationToken);
        return role == null ? null : MapToDto(role);
    }

    public async Task<OrganizationRoleDto?> CreateAsync(Guid organizationId, CreateOrganizationRoleDto dto, CancellationToken cancellationToken = default)
    {
        var permissions = dto.Permissions?.Where(OrganizationPermissions.IsValid).Distinct().ToList() ?? new List<string>();
        var role = new OrganizationCustomRole
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = dto.Name.Trim(),
            PermissionsJson = JsonSerializer.Serialize(permissions)
        };
        _context.OrganizationCustomRoles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);
        return MapToDto(role);
    }

    public async Task<OrganizationRoleDto?> UpdateAsync(Guid id, Guid organizationId, UpdateOrganizationRoleDto dto, CancellationToken cancellationToken = default)
    {
        var role = await _context.OrganizationCustomRoles
            .FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == organizationId, cancellationToken);
        if (role == null) return null;

        role.Name = dto.Name.Trim();
        var permissions = dto.Permissions?.Where(OrganizationPermissions.IsValid).Distinct().ToList() ?? new List<string>();
        role.PermissionsJson = JsonSerializer.Serialize(permissions);
        await _context.SaveChangesAsync(cancellationToken);
        return MapToDto(role);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var role = await _context.OrganizationCustomRoles
            .FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == organizationId, cancellationToken);
        if (role == null) return false;

        var inUse = await _context.OrganizationMembers
            .AnyAsync(m => m.CustomRoleId == id && m.IsActive, cancellationToken);
        if (inUse) return false;

        _context.OrganizationCustomRoles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static OrganizationRoleDto MapToDto(OrganizationCustomRole r)
    {
        var permissions = new List<string>();
        if (!string.IsNullOrWhiteSpace(r.PermissionsJson))
        {
            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(r.PermissionsJson);
                if (list != null) permissions = list;
            }
            catch { /* ignore */ }
        }
        return new OrganizationRoleDto
        {
            Id = r.Id,
            OrganizationId = r.OrganizationId,
            Name = r.Name,
            Permissions = permissions
        };
    }
}
