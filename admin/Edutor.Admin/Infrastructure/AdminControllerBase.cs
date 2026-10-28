using FluentValidation.Results;
using Edutor.Core.Common.Bulk;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Infrastructure;

/// <summary>Admin controller-ləri üçün ortaq sahiblik, validasiya və toplu əməliyyat köməkçiləri.</summary>
public abstract class AdminControllerBase : Controller
{
    /// <summary>Admin/superadmin bütün tenant-ı görür; teacher yalnız öz qeydləri ilə işləyir.</summary>
    protected long? OwnerScope => User.IsAdmin() ? null : User.CurrentUserId();

    protected static object Invalid(ValidationResult result)
        => new
        {
            errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
        };

    protected async Task<IActionResult> BulkAsync(BulkRequest request,
        Func<BulkRequest, long?, CancellationToken, Task> apply, CancellationToken ct)
    {
        // Bərpa yalnız adminin əməliyyatıdır — silinmişləri yalnız o görür.
        if (request.Action == BulkActions.Restore && !User.IsAdmin()) return Forbid();
        await apply(request, OwnerScope, ct);
        return Json(new { ok = true });
    }

    protected async Task<IActionResult> BulkAsync(BulkRequest request,
        Func<BulkRequest, CancellationToken, Task> apply, CancellationToken ct)
    {
        if (request.Action == BulkActions.Restore && !User.IsAdmin()) return Forbid();
        await apply(request, ct);
        return Json(new { ok = true });
    }
}
