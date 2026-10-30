using Microsoft.AspNetCore.Mvc;

namespace Edutor.Core.Modules.TenantConfig.Requests;

public class TenantConfigUpsertRequest
{
    [FromForm(Name = "payment_block_mode")]
    public string PaymentBlockMode { get; set; } = Entities.PaymentBlockModes.None;

    [FromForm(Name = "payment_blocked_group_ids")]
    public List<long> PaymentBlockedGroupIds { get; set; } = [];
}
