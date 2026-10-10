using LanguageLMS.Core.Modules.Profession.Requests;
using LanguageLMS.Core.Modules.Profession.Validators;
using Microsoft.AspNetCore.Http;

namespace LanguageLMS.Tests.Profession;

public class ProfessionValidatorTests
{
    private readonly ProfessionUpsertRequestValidator _validator = new();

    private static IFormFile File(string name, int size)
        => new FormFile(new MemoryStream(new byte[size]), 0, size, "Image", name);

    [Fact]
    public void Empty_name_is_invalid()
        => Assert.False(_validator.Validate(new ProfessionUpsertRequest { Name = "" }).IsValid);

    [Fact]
    public void Name_longer_than_190_is_invalid()
        => Assert.False(_validator.Validate(new ProfessionUpsertRequest { Name = new string('a', 191) }).IsValid);

    [Fact]
    public void Valid_name_passes()
        => Assert.True(_validator.Validate(new ProfessionUpsertRequest { Name = "Chemistry" }).IsValid);

    [Theory]
    [InlineData("photo.jpg", true)]
    [InlineData("photo.png", true)]
    [InlineData("photo.gif", false)]
    public void Image_extension_is_validated(string fileName, bool expected)
        => Assert.Equal(expected, _validator
            .Validate(new ProfessionUpsertRequest { Name = "Ok", Image = File(fileName, 10) })
            .IsValid);
}
