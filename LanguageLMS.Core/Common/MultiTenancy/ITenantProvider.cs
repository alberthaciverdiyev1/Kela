namespace LanguageLMS.Core.Common.MultiTenancy;

public interface ITenantProvider
{
    long? TenantId { get; }
}
