using Automation.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Automation.Identity.Infrastructure.Auth;

namespace Automation.Identity.Features.Roles.UpdateRolePermissions;

public class UpdateRolePermissionsHandler(
    RoleManager<Role> roleManager,
    IPermissionService permissionService)
{
    public async Task<Result> Handle(
        UpdateRolePermissionsCommand request,
        CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.Id.ToString());
        if (role is null)
        {
            return Result.Fail($"Role with ID {request.Id} not found");
        }

        var oldStamp = role.ConcurrencyStamp;

        var existingClaims = await roleManager.GetClaimsAsync(role);
        var permissionClaims = existingClaims.Where(c => c.Type == "Permission").ToList();

        // Remove old permissions
        foreach (var claim in permissionClaims)
        {
            await roleManager.RemoveClaimAsync(role, claim);
        }

        // Add new permissions
        var newPermissions = request.Permissions?.Distinct().ToList() ?? [];
        foreach (var permission in newPermissions)
        {
            await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
        }

        // Reload role để lấy ConcurrencyStamp ổn định sau tất cả thay đổi
        // (mỗi Add/RemoveClaimAsync đều tự UpdateAsync và đổi stamp)
        var reloadedRole = await roleManager.FindByIdAsync(request.Id.ToString());
        if (reloadedRole is null)
        {
            return Result.Fail("Role not found after update");
        }

        var newStamp = reloadedRole.ConcurrencyStamp;

        // Xoá cache phiên bản cũ (keyed bằng oldStamp)
        if (!string.IsNullOrEmpty(oldStamp))
        {
            await permissionService.ClearRolePermissionsCacheAsync(reloadedRole.Id, oldStamp, cancellationToken);
        }

        // Ghi cache phiên bản mới (keyed bằng newStamp)
        if (!string.IsNullOrEmpty(newStamp))
        {
            await permissionService.CacheRolePermissionsAsync(reloadedRole.Id, newStamp, newPermissions, cancellationToken);
        }

        return Result.Ok();
    }
}


