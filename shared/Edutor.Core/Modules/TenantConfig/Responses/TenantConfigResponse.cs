namespace Edutor.Core.Modules.TenantConfig.Responses;

public record TenantConfigResponse(string PaymentBlockMode, IReadOnlyList<long> PaymentBlockedGroupIds);
