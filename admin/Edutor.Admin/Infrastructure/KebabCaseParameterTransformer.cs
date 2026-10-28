using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;

namespace Edutor.Admin.Infrastructure;

public sealed class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
        => value is null ? null : Regex.Replace(value.ToString()!, "([a-z0-9])([A-Z])", "$1-$2").ToLowerInvariant();
}
