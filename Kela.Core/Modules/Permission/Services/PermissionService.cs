using Kela.Core.Data;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Permission.Requests;
using Kela.Core.Modules.Permission.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Permission.Services;

using Permission = Kela.Core.Modules.Permission.Entities.Permission;
using RolePermission = Kela.Core.Modules.Permission.Entities.RolePermission;

public sealed class PermissionService(AppDbContext db) : IPermissionService
{
    public async Task<IReadOnlyList<PermissionResponse>> ListAsync(CancellationToken ct = default)
    {
        var permissions = await db.Permissions.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
        var links = await db.RolePermissions.AsNoTracking().Include(x => x.Permission).ToListAsync(ct);

        return permissions
            .Select(p => new PermissionResponse(
                p.Name,
                links.Where(l => l.PermissionId == p.Id).Select(l => l.Role).Distinct().ToList()))
            .ToList();
    }

    public async Task<PermissionResponse> CreateAsync(CreatePermissionRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        if (await db.Permissions.AnyAsync(x => x.Name == name, ct))
        {
            throw new ArgumentException("A permission with this name already exists");
        }

        var permission = new Permission { Name = name };
        db.Permissions.Add(permission);
        await db.SaveChangesAsync(ct);

        return new PermissionResponse(permission.Name, []);
    }

    public async Task SetRolePermissionsAsync(SetRolePermissionsRequest request, CancellationToken ct = default)
    {
        if (!AuthRoles.Student.Equals(request.Role) &&
            !AuthRoles.Teacher.Equals(request.Role) &&
            !AuthRoles.Admin.Equals(request.Role))
        {
            throw new ArgumentException("Role must be student, teacher or admin");
        }

        var wanted = await db.Permissions.Where(p => request.Permissions.Contains(p.Name)).ToListAsync(ct);
        var wantedIds = wanted.Select(p => p.Id).ToHashSet();

        var existing = await db.RolePermissions.Where(rp => rp.Role == request.Role).ToListAsync(ct);

        foreach (var link in existing.Where(l => !wantedIds.Contains(l.PermissionId)))
        {
            db.RolePermissions.Remove(link);
        }

        foreach (var id in wantedIds.Where(id => existing.All(l => l.PermissionId != id)))
        {
            db.RolePermissions.Add(new RolePermission { Role = request.Role, PermissionId = id });
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> HasAsync(string role, string permissionName, CancellationToken ct = default)
    {
        if (role == AuthRoles.Admin) return true;

        var rolePermissions = await db.RolePermissions
            .AsNoTracking()
            .Include(x => x.Permission)
            .Where(x => x.Role == role)
            .Select(x => x.Permission!.Name)
            .ToListAsync(ct);

        // rol üçün heç bir icazə təyin olunmayıbsa → məhdudiyyət yox
        if (rolePermissions.Count == 0) return true;

        return rolePermissions.Contains(permissionName);
    }
}
