namespace Kela.Core.Common.MultiTenancy;

public interface ITenantProvider
{
    long? TenantId { get; }
}
