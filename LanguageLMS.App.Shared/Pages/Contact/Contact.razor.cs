using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Contact;

public partial class Contact
{
    private bool _loading = true;
    private ContactDto? _contact;

    private IEnumerable<string> ContactNumbers =>
        new[] { _contact?.ContactNumber1, _contact?.ContactNumber2, _contact?.ContactNumber3, _contact?.ContactNumber4 }
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!);

    protected override async Task OnInitializedAsync()
    {
        try { _contact = await Api.ContactAsync(); } catch { }
        _loading = false;
    }

    private static string WhatsappUrl(string number)
        => $"https://wa.me/{new string(number.Where(char.IsDigit).ToArray())}";
}
