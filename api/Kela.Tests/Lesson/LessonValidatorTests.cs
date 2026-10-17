using Kela.Core.Modules.Lesson.Requests;
using Kela.Core.Modules.Lesson.Validators;

namespace Kela.Tests.Lesson;

public class LessonValidatorTests
{
    private readonly LessonUpsertRequestValidator _validator = new(new TestLocalizer());

    [Fact]
    public void Empty_title_is_invalid()
        => Assert.False(_validator.Validate(new LessonUpsertRequest { Title = "", SubjectId = 1 }).IsValid);

    [Fact]
    public void Missing_subject_is_invalid()
        => Assert.False(_validator.Validate(new LessonUpsertRequest { Title = "Lesson" }).IsValid);

    [Fact]
    public void Valid_request_passes()
        => Assert.True(_validator.Validate(new LessonUpsertRequest { Title = "Lesson", SubjectId = 1 }).IsValid);
}
