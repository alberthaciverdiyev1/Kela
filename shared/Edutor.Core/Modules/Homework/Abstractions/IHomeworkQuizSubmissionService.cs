namespace Edutor.Core.Modules.Homework.Abstractions;

/// <summary>
/// Quizli ev tapşırıqlarında ayrıca təqdimat tələb olunmur: bütün quizlər tamamlananda
/// tapşırıq avtomatik "təqdim edilib" statusuna keçir.
/// </summary>
public interface IHomeworkQuizSubmissionService
{
    Task SyncAfterQuizCompletionAsync(long studentId, long quizId, CancellationToken ct = default);

    Task SyncHomeworkAsync(long studentId, long homeworkId, CancellationToken ct = default);
}
