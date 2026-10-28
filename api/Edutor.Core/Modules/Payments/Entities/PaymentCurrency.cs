namespace Edutor.Core.Modules.Payments.Entities;

/// <summary>Dəstəklənən ödəniş valyutaları (ISO 4217 kodu).</summary>
public enum PaymentCurrency
{
    AZN,
    USD,
    EUR,
    TRY,
    GBP,
    RUB
}

public static class PaymentCurrencies
{
    public static readonly string[] All = Enum.GetNames<PaymentCurrency>();

    public static string Normalize(string? code)
    {
        var value = (code ?? string.Empty).Trim().ToUpperInvariant();
        return Enum.TryParse<PaymentCurrency>(value, ignoreCase: false, out _) ? value : nameof(PaymentCurrency.AZN);
    }
}
