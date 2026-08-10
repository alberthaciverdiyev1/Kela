using FluentValidation;
using Kela.Application.Features.Contents.Requests;
using Kela.Application.Features.Contents.Responses;
using Kela.Application.Features.Nodes;
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
    }

    public async Task SetPublishedAsync(int id, bool published, CancellationToken cancellationToken = default)
    {
        var content = await contents.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan içerik bulunamadı.");

        content.IsPublished = published;
        content.UpdatedAt = DateTime.UtcNow;

        contents.Update(content);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var content = await contents.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan içerik bulunamadı.");

        var now = DateTime.UtcNow;
        content.DeletedAt = now;

        var refNodes = await nodes.GetByContentIdAsync(content.Id, cancellationToken);
        foreach (var node in refNodes)
        {
            node.DeletedAt = now;
        }

        contents.Update(content);
        await unitOfWork.SaveChangesAsync(cancellationToken);
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
