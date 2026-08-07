using System.Reflection;
using System.Text.Json;
using Kela.Web.Helpers;

namespace Kela.Web.Localization;

public sealed class Localizer(IHttpContextAccessor contextAccessor)
{
    private static readonly Dictionary<string, Dictionary<string, string>> Dictionaries = LoadDictionaries();

    public string Active
    {
        get
        {
            var lang = contextAccessor.HttpContext?.Request.Cookies[AppConstants.LangCookie];
            return lang is not null && Dictionaries.ContainsKey(lang) ? lang : AppConstants.DefaultLang;
        }
    }

    public string T(string key, object? parameters = null)
    {
        if (Dictionaries.TryGetValue(Active, out var dict) && dict.TryGetValue(key, out var value))
        {
            return Interpolate(value, parameters);
        }

        if (Dictionaries.TryGetValue("en", out var en) && en.TryGetValue(key, out var enValue))
        {
            return Interpolate(enValue, parameters);
        }

        return key;
    }

    private static string Interpolate(string template, object? parameters)
    {
        if (parameters is null)
        {
            return template;
        }

        var result = template;
        foreach (var prop in parameters.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            result = result.Replace("{" + prop.Name + "}", prop.GetValue(parameters)?.ToString());
        }

        return result;
    }

    private static Dictionary<string, Dictionary<string, string>> LoadDictionaries()
    {
        var result = new Dictionary<string, Dictionary<string, string>>();
        var assembly = typeof(Localizer).Assembly;

        foreach (var lang in AppConstants.Langs)
        {
            using var stream = assembly.GetManifestResourceStream($"Kela.Web.Localization.messages_{lang}.json");
            if (stream is null)
            {
                continue;
            }

            using var reader = new StreamReader(stream);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd());
            if (dict is not null)
            {
                result[lang] = dict;
            }
        }

        return result;
    }
}
