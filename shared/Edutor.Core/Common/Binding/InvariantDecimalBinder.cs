using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Edutor.Core.Common.Binding;

/// <summary>
/// HTML <c>type="number"</c> həmişə nöqtə ilə göndərir, server isə cari kulturaya
/// görə parse edir — az/tr lokalında "249.5" 2495 kimi oxunur. Bu binder əvvəl
/// invariant kultura ilə, alınmasa cari kultura ilə oxuyur.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class InvariantDecimalAttribute : Attribute, IModelNameProvider, IBinderTypeProviderMetadata
{
    public Type BinderType => typeof(InvariantDecimalBinder);
    public BindingSource BindingSource => BindingSource.Form;
    public string? Name { get; set; }
}

public sealed class InvariantDecimalBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context)
    {
        var result = context.ValueProvider.GetValue(context.ModelName);
        if (result == ValueProviderResult.None) return Task.CompletedTask;

        context.ModelState.SetModelValue(context.ModelName, result);

        var raw = result.FirstValue?.Trim();
        if (string.IsNullOrEmpty(raw))
        {
            if (context.ModelMetadata.IsNullableValueType) context.Result = ModelBindingResult.Success(null);
            else context.ModelState.TryAddModelError(context.ModelName, "Value is required.");

            return Task.CompletedTask;
        }

        var normalized = raw.Replace(" ", string.Empty);
        if (normalized.Contains('.') && normalized.Contains(','))
        {
            normalized = normalized.Replace(",", string.Empty);
        }
        else if (normalized.Contains(','))
        {
            normalized = normalized.Replace(',', '.');
        }

        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            && !decimal.TryParse(raw, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
        {
            context.ModelState.TryAddModelError(context.ModelName, "Invalid number.");
            return Task.CompletedTask;
        }

        context.Result = ModelBindingResult.Success(value);
        return Task.CompletedTask;
    }
}
