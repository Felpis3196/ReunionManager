using SmartMeetingManager.Application.DTOs;

namespace SmartMeetingManager.Application.Interfaces;

public interface IOrganizationRoleService
{
    Task<List<OrganizationRoleDto>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<OrganizationRoleDto?> GetByIdAsync(Guid id, Guid organizationId, CancellationToken cancellationToken = default);
    Task<OrganizationRoleDto?> CreateAsync(Guid organizationId, CreateOrganizationRoleDto dto, CancellationToken cancellationToken = default);
    Task<OrganizationRoleDto?> UpdateAsync(Guid id, Guid organizationId, UpdateOrganizationRoleDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, Guid organizationId, CancellationToken cancellationToken = default);
}
