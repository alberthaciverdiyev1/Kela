using System.Security.Claims;
using System.Text.Encodings.Web;
using Kela.Core.Modules.Nodes.Abstractions;
using Kela.Core.Modules.Nodes.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Kela.Api.Modules.Nodes.Auth;

public sealed class NodeTokenAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string Scheme = "NodeToken";

    /// <summary>Worker bu başlıqla öz access token-ini göndərir.</summary>
    public const string Header = "X-Node-Token";
}

/// <summary>
/// Worker cihazlarını JWT-dən ayrı sxemlə autentifikasiya edir. Token hash-i bazada
/// saxlanılır və hər sorğuda yoxlanılır ki, ləğv dərhal təsir etsin.
/// </summary>
public sealed class NodeTokenAuthenticationHandler(
    IOptionsMonitor<NodeTokenAuthenticationOptions> options,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder,
    INodeService nodes)
    : AuthenticationHandler<NodeTokenAuthenticationOptions>(options, loggerFactory, encoder)
{
    /// <summary>Autentifikasiya olunmuş cihaz sorğu boyu burada saxlanılır (təkrar sorğu olmasın).</summary>
    public const string NodeItemKey = "Nodes.WorkerNode";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(NodeTokenAuthenticationOptions.Header, out var header)) return AuthenticateResult.NoResult();

        var token = header.ToString();
        if (string.IsNullOrWhiteSpace(token)) return AuthenticateResult.NoResult();

        var node = await nodes.AuthenticateAsync(token, Context.RequestAborted);
        if (node is null) return AuthenticateResult.Fail("Invalid or revoked node token");

        Context.Items[NodeItemKey] = node;

        var claims = new[]
        {
            new Claim("node_id", node.Id.ToString()),
            new Claim("tenant_id", node.TenantId.ToString()),
            new Claim(ClaimTypes.Name, node.Name)
        };

        var identity = new ClaimsIdentity(claims, NodeTokenAuthenticationOptions.Scheme);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), NodeTokenAuthenticationOptions.Scheme));
    }
}

public static class NodeContextExtensions
{
    /// <summary>Autentifikasiya zamanı tapılmış cihazı qaytarır (əlavə sorğu olmadan).</summary>
    public static WorkerNode WorkerNode(this HttpContext context)
        => (WorkerNode)context.Items[NodeTokenAuthenticationHandler.NodeItemKey]!;
}
