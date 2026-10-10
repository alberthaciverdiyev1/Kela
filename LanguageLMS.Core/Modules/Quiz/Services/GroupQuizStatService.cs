using ClosedXML.Excel;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.Quiz.Requests;
using LanguageLMS.Core.Modules.Quiz.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.Quiz.Services;

using StudentQuizQuestion = LanguageLMS.Core.Modules.Quiz.Entities.StudentQuizQuestion;

public sealed class GroupQuizStatService(AppDbContext db) : IGroupQuizStatService
{
    public async Task<GroupQuizStatResponse> GetAsync(
        long groupId, GroupQuizStatRequest request, CancellationToken ct = default)
    {
        if (!await db.Groups.AnyAsync(g => g.Id == groupId, ct))
        {
            throw new KeyNotFoundException($"Group not found: {groupId}");
        }

        var subjectIds = await db.GroupSubjects
            .Where(gs => gs.GroupId == groupId)
            .Select(gs => gs.SubjectId)
            .ToListAsync(ct);

        var lessonIds = await db.Lessons
            .Where(l => subjectIds.Contains(l.SubjectId))
            .Select(l => l.Id)
            .ToListAsync(ct);

        var lessonQuizIds = await db.Quizzes
            .Where(q => q.LessonId != null && lessonIds.Contains(q.LessonId.Value))
            .Select(q => q.Id)
            .ToListAsync(ct);

        var generalQuizIds = await db.GroupQuizzes
            .Where(gq => gq.GroupId == groupId)
            .Select(gq => gq.QuizId)
            .ToListAsync(ct);

        var quizIds = lessonQuizIds.Union(generalQuizIds).Distinct().ToList();

        if (request.QuizType == "lesson")
        {
            quizIds = quizIds.Intersect(lessonQuizIds).ToList();
        }
        else if (request.QuizType == "general")
        {
            quizIds = quizIds.Intersect(generalQuizIds).ToList();
        }

        var totalQuestions = quizIds.Count == 0
            ? 0
            : await db.QuizQuestions.CountAsync(qq => quizIds.Contains(qq.QuizId), ct);

        var studentIds = await db.StudentGroups
            .Where(sg => sg.GroupId == groupId)
            .Select(sg => sg.StudentId)
            .ToListAsync(ct);

        var students = await db.Students.AsNoTracking()
            .Where(s => studentIds.Contains(s.Id))
            .OrderBy(s => s.FullName)
            .Select(s => new { s.Id, s.FullName, s.Email })
            .ToListAsync(ct);

        var attempts = quizIds.Count == 0
            ? []
            : await db.StudentQuizzes.AsNoTracking()
                .Where(sq => studentIds.Contains(sq.StudentId) && quizIds.Contains(sq.QuizId))
                .ToListAsync(ct);

        var attemptIds = attempts.Select(a => a.Id).ToList();

        var answers = attemptIds.Count == 0
            ? new List<StudentQuizQuestion>()
            : await db.StudentQuizQuestions.AsNoTracking()
                .Where(a => attemptIds.Contains(a.StudentQuizId))
                .ToListAsync(ct);

        var stats = students.Select(student =>
        {
            var studentAttempts = attempts.Where(a => a.StudentId == student.Id).ToList();
            var attemptedQuizCount = studentAttempts.Select(a => a.QuizId).Distinct().Count();
            var completedQuestions = studentAttempts.Sum(a => a.CompletedQuestionCount);
            var attemptIdSet = studentAttempts.Select(a => a.Id).ToHashSet();

            var studentAnswers = answers.Where(a => attemptIdSet.Contains(a.StudentQuizId)).ToList();
            var correct = studentAnswers.Count(a => a.IsRight);
            var wrong = studentAnswers.Count(a => !a.IsRight);

            return new StudentQuizStat(
                student.Id,
                student.FullName,
                student.Email,
                quizIds.Count,
                attemptedQuizCount,
                quizIds.Count - attemptedQuizCount,
                totalQuestions,
                completedQuestions,
                correct,
                wrong,
                totalQuestions > 0 ? Math.Round(correct * 100.0 / totalQuestions, 1) : 0);
        }).ToList();

        return new GroupQuizStatResponse(groupId, quizIds.Count, totalQuestions, stats);
    }

    public async Task<byte[]> ExportXlsxAsync(long groupId, GroupQuizStatRequest request, CancellationToken ct = default)
    {
        var stat = await GetAsync(groupId, request, ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("QuizStat");
        var headers = new[] { "StudentId", "FullName", "Email", "QuizCount", "Attempted", "NotAttempted", "TotalQuestions", "Completed", "Correct", "Wrong", "ScorePercent" };
        for (var c = 0; c < headers.Length; c++) ws.Cell(1, c + 1).Value = headers[c];
        ws.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var r in stat.Students)
        {
            ws.Cell(row, 1).Value = r.StudentId;
            ws.Cell(row, 2).Value = r.FullName;
            ws.Cell(row, 3).Value = r.Email;
            ws.Cell(row, 4).Value = r.QuizCount;
            ws.Cell(row, 5).Value = r.AttemptedQuizCount;
            ws.Cell(row, 6).Value = r.NotAttemptedQuizCount;
            ws.Cell(row, 7).Value = r.TotalQuestions;
            ws.Cell(row, 8).Value = r.CompletedQuestions;
            ws.Cell(row, 9).Value = r.Correct;
            ws.Cell(row, 10).Value = r.Wrong;
            ws.Cell(row, 11).Value = r.ScorePercent;
            row++;
        }

        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
