using Microsoft.AspNetCore.Components;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Contact;

public partial class Contact
{
    private bool _loading = true;
    private ContactDto? _course;
    private ContactDto? _platform;

    protected override async Task OnInitializedAsync()
    {
        try { _course = await Api.ContactAsync(); } catch { }
        try { _platform = await Api.PlatformContactAsync(); } catch { }
        _loading = false;
    }

    private static bool HasChannels(ContactDto? c)
        => c is not null && (
            !string.IsNullOrWhiteSpace(c.WhatsappNumber)
            || !string.IsNullOrWhiteSpace(c.InstagramUrl)
            || !string.IsNullOrWhiteSpace(c.FacebookUrl)
            || !string.IsNullOrWhiteSpace(c.Email)
            || !string.IsNullOrWhiteSpace(c.ContactNumber1)
            || !string.IsNullOrWhiteSpace(c.ContactNumber2)
            || !string.IsNullOrWhiteSpace(c.ContactNumber3)
            || !string.IsNullOrWhiteSpace(c.ContactNumber4));
}
