using Microsoft.AspNetCore.Components;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Payments;

public partial class Payments
{
    private bool _loading = true;
    private StudentPaymentOverviewDto? _payment;

    protected override void OnInitialized() => OnRefresh(FetchAsync);

    protected override async Task OnInitializedAsync()
    {
        await FetchAsync();
        _loading = false;
    }

    private async Task FetchAsync()
    {
        try { _payment = await Api.PaymentsAsync(); }
        catch { _payment = null; }
    }
}
