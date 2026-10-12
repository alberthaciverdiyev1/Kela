namespace LanguageLMS.App.Shared.Dtos.Progress;

public record StatisticsDto(StatisticsOverviewDto Overview, List<SubjectStatDto> BySubject);
