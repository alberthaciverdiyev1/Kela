namespace Kela.Core.Common.Responses;

public record PagedResponse<T>(
    IReadOnlyList<T> Data,
    int CurrentPage,
    int LastPage,
    int PerPage,
    int Total);
