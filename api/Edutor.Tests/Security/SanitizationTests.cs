using Edutor.Core.Modules.PrivacyPolicy.Abstractions;
using Edutor.Core.Modules.PrivacyPolicy.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Edutor.Tests.Security;

[Collection("api")]
public class SanitizationTests(ApiFactory factory)
{
    [Fact]
    public async Task Privacy_policy_html_strips_scripts_and_event_handlers()
    {
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPrivacyPolicyService>();

        var saved = await service.UpsertAsync(new UpsertPrivacyPolicyRequest
        {
            Type = $"terms-{Guid.NewGuid():N}",
            Content = "<p>Hello</p><script>alert(1)</script><img src=x onerror=\"alert(1)\">"
        });

        Assert.Contains("<p>Hello</p>", saved.Content);
        Assert.DoesNotContain("script", saved.Content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onerror", saved.Content, StringComparison.OrdinalIgnoreCase);
    }
}
