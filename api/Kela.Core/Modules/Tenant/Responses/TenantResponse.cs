namespace Kela.Core.Modules.Tenant.Responses;

public record TenantResponse(long Id, string Name, string Slug, bool IsActive, string? Logo = null, bool IsPending = false);
