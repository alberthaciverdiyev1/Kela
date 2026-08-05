namespace Kela.Application.Pagination;

/// <summary>
/// Sayfalı sorgu sonucu. Sınırsız GetAll yerine liste uçları bu yapıyla döner.
/// </summary>
/// <param name="Items">Geçerli sayfadaki öğeler.</param>
/// <param name="Page">İstenen sayfa (1 tabanlı).</param>
/// <param name="PageSize">Sayfa başına öğe sayısı.</param>
/// <param name="TotalCount">Toplam kayıt sayısı (filtreye göre).</param>
public sealed record PaginatedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
