using LanguageLMS.Core.Modules.Subject.Requests;
using LanguageLMS.Core.Modules.Subject.Validators;

namespace LanguageLMS.Tests.Subject;

public class SubjectValidatorTests
{
    private readonly SubjectUpsertRequestValidator _validator = new();

    [Fact]
    public void Empty_name_is_invalid()
        => Assert.False(_validator.Validate(new SubjectUpsertRequest { Name = "" }).IsValid);

    [Fact]
    public void Name_longer_than_190_is_invalid()
        => Assert.False(_validator.Validate(new SubjectUpsertRequest { Name = new string('a', 191) }).IsValid);

    [Fact]
    public void Valid_name_passes()
        => Assert.True(_validator.Validate(new SubjectUpsertRequest { Name = "Mathematics" }).IsValid);
}
