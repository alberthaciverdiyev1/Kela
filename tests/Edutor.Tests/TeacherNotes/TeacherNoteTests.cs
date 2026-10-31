using Edutor.Core.Data;
using Edutor.Core.Modules.TeacherNote.Abstractions;
using Edutor.Core.Modules.TeacherNote.Entities;
using Edutor.Core.Modules.TeacherNote.Requests;
using Microsoft.Extensions.DependencyInjection;
using UserTypes = Edutor.Core.Modules.User.Entities.UserTypes;

namespace Edutor.Tests.TeacherNotes;

/// <summary>Admin paneldəki şəxsi qeydlər: öz qeydləri + seçilmiş həmkarlarla paylaşım.</summary>
[Collection("api")]
public class TeacherNoteTests(ApiFactory factory)
{
    private async Task<T> With<TService, T>(Func<TService, Task<T>> action) where TService : notnull
    {
        using var scope = factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private Task<T> Note<T>(Func<ITeacherNoteService, Task<T>> action) => With<ITeacherNoteService, T>(action);

    private async Task NoteVoid(Func<ITeacherNoteService, Task> action)
    {
        using var scope = factory.Services.CreateScope();
        await action(scope.ServiceProvider.GetRequiredService<ITeacherNoteService>());
    }

    private async Task<long> CreateTeacherAsync(string? name = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = new Core.Modules.User.Entities.User
        {
            Name = name ?? "Teacher",
            Surname = Guid.NewGuid().ToString("N")[..6],
            Email = $"{Guid.NewGuid():N}@test.local",
            PasswordHash = "x",
            Type = UserTypes.Teacher
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    private static TeacherNoteUpsertRequest Request(string title, string visibility = TeacherNoteVisibility.Private,
        params long[] sharedWith) => new()
        {
            Title = title,
            Body = "Body",
            Visibility = visibility,
            SharedWithUserIds = sharedWith.ToList()
        };

    private async Task<long> CreateContentAsync()
    {
        var subjectId = await TestData.SubjectAsync(factory, $"S-{Guid.NewGuid():N}");
        var lessonId = await TestData.LessonAsync(factory, subjectId, $"L-{Guid.NewGuid():N}");
        return await TestData.LessonContentAsync(factory, lessonId, $"C-{Guid.NewGuid():N}");
    }

    [Fact]
    public async Task Private_note_is_visible_only_to_its_author()
    {
        var owner = await CreateTeacherAsync();
        var other = await CreateTeacherAsync();

        var created = await Note(s => s.CreateAsync(owner, Request("Private one")));
        Assert.True(created.IsMine);

        var mine = await Note(s => s.ListAsync(owner, new TeacherNoteListRequest()));
        Assert.Contains(mine.Data, x => x.Id == created.Id);

        var others = await Note(s => s.ListAsync(other, new TeacherNoteListRequest()));
        Assert.DoesNotContain(others.Data, x => x.Id == created.Id);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await Note(s => s.GetAsync(other, created.Id)));
    }

    [Fact]
    public async Task Shared_note_is_listed_only_for_selected_users()
    {
        var owner = await CreateTeacherAsync("Owner");
        var target = await CreateTeacherAsync("Target");
        var outsider = await CreateTeacherAsync("Outsider");

        var created = await Note(s => s.CreateAsync(owner, Request("Shared one", TeacherNoteVisibility.Shared, target)));

        var shared = Assert.Single((await Note(s => s.ListAsync(target, new TeacherNoteListRequest { Scope = "shared" }))).Data);
        Assert.Equal(created.Id, shared.Id);
        Assert.False(shared.IsMine);
        Assert.Contains(shared.SharedWith, u => u.Id == target);

        Assert.Empty((await Note(s => s.ListAsync(outsider, new TeacherNoteListRequest()))).Data);
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await Note(s => s.GetAsync(target, created.Id)));
    }

    [Fact]
    public async Task Switching_back_to_private_removes_shares()
    {
        var owner = await CreateTeacherAsync();
        var target = await CreateTeacherAsync();

        var created = await Note(s => s.CreateAsync(owner, Request("Shared", TeacherNoteVisibility.Shared, target)));

        await Note(s => s.UpdateAsync(owner, created.Id, Request("Now private")));

        Assert.Empty((await Note(s => s.ListAsync(target, new TeacherNoteListRequest()))).Data);
    }

    [Fact]
    public async Task Only_the_owner_can_update_or_delete()
    {
        var owner = await CreateTeacherAsync();
        var target = await CreateTeacherAsync();

        var created = await Note(s => s.CreateAsync(owner, Request("Mine", TeacherNoteVisibility.Shared, target)));

        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await Note(s => s.UpdateAsync(target, created.Id, Request("Hijack"))));
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await NoteVoid(s => s.DeleteAsync(target, created.Id)));
    }

    [Fact]
    public async Task Shared_note_requires_a_valid_share_target()
    {
        var owner = await CreateTeacherAsync();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await Note(s => s.CreateAsync(owner, Request("No target", TeacherNoteVisibility.Shared))));

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await Note(s => s.CreateAsync(owner, Request("Bad target", TeacherNoteVisibility.Shared, 999_999))));
    }

    [Fact]
    public async Task Invalid_share_target_leaves_no_note_behind()
    {
        var owner = await CreateTeacherAsync();
        var title = $"Ghost-{Guid.NewGuid():N}";

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await Note(s => s.CreateAsync(owner, Request(title, TeacherNoteVisibility.Shared, 999_999))));

        var notes = await Note(s => s.ListAsync(owner, new TeacherNoteListRequest()));
        Assert.DoesNotContain(notes.Data, x => x.Title == title);
    }

    [Fact]
    public async Task Share_targets_exclude_students_and_the_author()
    {
        var owner = await CreateTeacherAsync();
        var target = await CreateTeacherAsync();
        var contentId = await CreateContentAsync();

        var targets = await Note(s => s.ListShareTargetsAsync(owner));
        Assert.Contains(targets, u => u.Id == target);
        Assert.DoesNotContain(targets, u => u.Id == owner);

        var contents = await Note(s => s.ListContentOptionsAsync());
        Assert.Contains(contents, c => c.Id == contentId);
    }

    [Fact]
    public async Task Content_link_and_timestamp_are_stored()
    {
        var owner = await CreateTeacherAsync();
        var contentId = await CreateContentAsync();

        var created = await Note(s => s.CreateAsync(owner, new TeacherNoteUpsertRequest
        {
            Title = "With material",
            ContentId = contentId,
            PositionSeconds = 75
        }));

        Assert.Equal(contentId, created.ContentId);
        Assert.Equal(75, created.PositionSeconds);
        Assert.False(string.IsNullOrWhiteSpace(created.ContentTitle));
        Assert.NotNull(created.LessonTitle);
    }
}
