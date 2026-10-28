namespace Edutor.Core.Modules.Tenant.Requests;

/// <summary>Tenant (kurs) admininin öz kursu üçün dəyişə bildiyi sahələr.</summary>
public class UpdateTenantProfileRequest
{
    public string Name { get; set; } = string.Empty;
}
