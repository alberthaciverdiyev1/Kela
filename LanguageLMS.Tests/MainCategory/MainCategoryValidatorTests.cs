using LanguageLMS.Core.Modules.MainCategory.Requests;
using LanguageLMS.Core.Modules.MainCategory.Validators;

namespace LanguageLMS.Tests.MainCategory;

public class MainCategoryValidatorTests
{
    private readonly MainCategoryUpsertRequestValidator _validator = new();

    [Fact]
    public void Empty_name_is_invalid()
        => Assert.False(_validator.Validate(new MainCategoryUpsertRequest { Name = "" }).IsValid);

    [Fact]
    public void Name_longer_than_190_is_invalid()
        => Assert.False(_validator.Validate(new MainCategoryUpsertRequest { Name = new string('a', 191) }).IsValid);

    [Fact]
    public void Valid_name_passes()
        => Assert.True(_validator.Validate(new MainCategoryUpsertRequest { Name = "Mathematics" }).IsValid);
}
