namespace LanguageLMS.Core.Modules.Profession.Responses;

public record ProfessionRef(long Id, string Name, string Image);

public record ProfessionResponse(
    long Id,
    string Name,
    string Image,
    long? ParentId,
    IReadOnlyList<ProfessionRef> SubCategories);
