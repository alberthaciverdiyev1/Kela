using Microsoft.AspNetCore.Components;
using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Pages.Payments;

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
