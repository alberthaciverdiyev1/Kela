namespace Kela.Web.Helpers;

public sealed class RelayCookieHandler(IHttpContextAccessor contextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var ctx = contextAccessor.HttpContext;
        if (ctx is not null)
        {
            var lang = ctx.Request.Cookies[AppConstants.LangCookie] ?? AppConstants.DefaultLang;

            var uri = request.RequestUri!;
            var query = uri.Query.Length > 1 ? uri.Query[1..] : "";
            var newQuery = string.IsNullOrEmpty(query) ? $"lang={lang}" : $"{query}&lang={lang}";
            request.RequestUri = new UriBuilder(uri) { Query = newQuery }.Uri;
            request.Headers.TryAddWithoutValidation("Accept-Language", lang);

            var authCookie = ctx.Request.Cookies[AppConstants.ApiAuthCookie];
            if (!string.IsNullOrEmpty(authCookie))
            {
                request.Headers.TryAddWithoutValidation("Cookie", $"{AppConstants.ApiAuthCookie}={authCookie}");
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}
