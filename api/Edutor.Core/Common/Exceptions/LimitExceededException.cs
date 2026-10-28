namespace Edutor.Core.Common.Exceptions;

/// <summary>
/// Plan limiti dolub. Adi 403-dən fərqi: panel müştəriyə uyğun əlavə paketi
/// təklif edə bilsin deyə resurs növü və istifadə/limit rəqəmləri daşınır.
/// </summary>
public sealed class LimitExceededException(string kind, string message, long used, long limit)
    : Exception(message)
{
    public string Kind { get; } = kind;

    public long Used { get; } = used;

    public long Limit { get; } = limit;
}
