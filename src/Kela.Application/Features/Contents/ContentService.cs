using FluentValidation;
using Kela.Application.Features.Contents.Requests;
using Kela.Application.Features.Contents.Responses;
using Kela.Application.Features.Lessons;
using Kela.Application.Features.Nodes;
using Kela.Application.Features.Quizzes;
using Kela.Application.Features.Users;
using Kela.Application.Patterns;
using Kela.Domain.Common;
using Kela.Domain.Entities;
using Kela.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Kela.Application.Features.Contents;

internal sealed class ContentService(
    IContentRepository contents,
    INodeRepository nodes,
    IQuizRepository quizzes,
    ILessonRepository lessons,
    IUserRepository users,
    UserManager<User> userManager,
    IUnitOfWork unitOfWork,
    IValidator<CreateContentRequest> createValidator,
    IValidator<UpdateContentRequest> updateValidator) : IContentService
{
    public async Task<List<ContentResponse>> GetByTeacherAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        var items = await contents.GetByTeacherAsync(teacherId, cancellationToken);
        return items.Select(c => c.ToResponse()).ToList();
    }

    public async Task<ContentResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var content = await contents.GetByIdAsync(id, cancellationToken);
        return content?.ToResponse();
    }

    public async Task<int> CreateAsync(CreateContentRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        await EnsureTeacherExistsAsync(request.TeacherId, cancellationToken);

        var trimmed = request.Title.Trim();
        var content = new Content
        {
            TeacherId = request.TeacherId,
            Title = trimmed,
            Description = request.Description,
            Type = request.Type,
            Url = request.Url,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
        };

        contents.Add(content);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var library = await nodes.GetByContextAsync(null, request.TeacherId, cancellationToken);
        NodeTree.EnsureParent(library, request.ParentId, null, request.TeacherId);

        var sibling = library.Where(n => n.ParentId == request.ParentId).ToList();
        var node = new Node
        {
            TeacherId = request.TeacherId,
            ParentId = request.ParentId,
            Name = trimmed,
            Kind = NodeType.Content,
            ContentId = content.Id,
            Position = NodeTree.NextPosition(sibling),
            CreatedAt = DateTime.UtcNow,
        };

        nodes.Add(node);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.Type == ContentType.Quiz)
        {
            quizzes.Add(new Quiz
            {
                ContentId = content.Id,
                TeacherId = request.TeacherId,
                Title = trimmed,
                Description = request.Description,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow,
            });
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else if (request.Type == ContentType.Lesson)
        {
            lessons.Add(new Lesson
            {
                ContentId = content.Id,
                TeacherId = request.TeacherId,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow,
            });
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return content.Id;
    }

    public async Task UpdateAsync(int id, UpdateContentRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var content = await contents.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan içerik bulunamadı.");

        content.Title = request.Title.Trim();
        content.Description = request.Description;
        content.Url = request.Url;
        content.UpdatedAt = DateTime.UtcNow;

        var refNodes = await nodes.GetByContentIdAsync(content.Id, cancellationToken);
        foreach (var node in refNodes)
        {
            node.Name = content.Title;
            node.UpdatedAt = DateTime.UtcNow;
        }

        contents.Update(content);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (content.Type == ContentType.Quiz)
        {
            var quiz = await quizzes.GetByContentIdAsync(content.Id, cancellationToken);
            if (quiz is not null)
            {
                quiz.Title = content.Title;
                quiz.Description = content.Description;
                quiz.UpdatedAt = DateTime.UtcNow;
                quizzes.Update(quiz);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task SetPublishedAsync(int id, bool published, CancellationToken cancellationToken = default)
    {
        var content = await contents.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan içerik bulunamadı.");

        content.IsPublished = published;
        content.UpdatedAt = DateTime.UtcNow;

        contents.Update(content);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (content.Type == ContentType.Lesson)
        {
            var lesson = await lessons.GetByContentIdAsync(content.Id, cancellationToken);
            if (lesson is not null)
            {
                lesson.IsPublished = published;
                lesson.UpdatedAt = DateTime.UtcNow;
                lessons.Update(lesson);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var content = await contents.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan içerik bulunamadı.");

        // Bağlı kayıtları içerik yumuşak-silmeden ÖNCE çek.
        // (LessonRepository.Content navigasyonuna uygulanan DeletedAt filtreleri,
        //  içerik silindikten sonra dersi de sorgudan eler — o zaman satır yetim kalır.)
        Lesson? lesson = null;
        Quiz? quiz = null;
        if (content.Type == ContentType.Quiz)
        {
            quiz = await quizzes.GetByContentIdAsync(content.Id, cancellationToken);
        }
        else if (content.Type == ContentType.Lesson)
        {
            lesson = await lessons.GetByContentIdAsync(content.Id, cancellationToken);
        }

        var now = DateTime.UtcNow;
        content.DeletedAt = now;

        var refNodes = await nodes.GetByContentIdAsync(content.Id, cancellationToken);
        foreach (var node in refNodes)
        {
            node.DeletedAt = now;
        }

        contents.Update(content);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (quiz is not null)
        {
            quizzes.Remove(quiz);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else if (lesson is not null)
        {
            lessons.Remove(lesson);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task EnsureTeacherExistsAsync(int teacherId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(teacherId, cancellationToken);
        if (user is null || !await userManager.IsInRoleAsync(user, RoleNames.Teacher))
        {
            throw new InvalidOperationException($"Id = {teacherId} olan öğretmen bulunamadı.");
        }
    }
}
