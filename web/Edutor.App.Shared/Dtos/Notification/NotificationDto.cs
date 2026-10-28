namespace Edutor.App.Shared.Dtos.Notification;

public record NotificationDto(
    long Id,
    string Title,
    string? Body,
    string? Image,
    bool All,
    DateTimeOffset CreatedAt,
    string? Data = null);

/// <summary>Serverin PagedResponse&lt;T&gt; zərfi ilə uyğun səhifələnmiş nəticə.</summary>
public record PagedResult<T>(
    IReadOnlyList<T> Data,
    int CurrentPage,
    int LastPage,
    int PerPage,
    int Total);
