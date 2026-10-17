using Kela.Core.Modules.Banner.Requests;
using Kela.Core.Modules.Banner.Validators;
using Microsoft.AspNetCore.Http;

namespace Kela.Tests.Banner;

public class BannerValidatorTests
{
    private readonly BannerUpsertRequestValidator _validator = new(new TestLocalizer());

    private static IFormFile File(string name, int size)
        => new FormFile(new MemoryStream(new byte[size]), 0, size, "Image", name);

    [Fact]
    public void Empty_title_is_invalid()
    {
        var result = _validator.Validate(new BannerUpsertRequest { Title = "" });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(BannerUpsertRequest.Title));
    }

    [Fact]
    public void Title_longer_than_190_is_invalid()
    {
        var result = _validator.Validate(new BannerUpsertRequest { Title = new string('a', 191) });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Valid_title_passes()
    {
        var result = _validator.Validate(new BannerUpsertRequest { Title = "Hello" });
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("photo.jpg", true)]
    [InlineData("photo.jpeg", true)]
    [InlineData("photo.png", true)]
    [InlineData("photo.gif", false)]
    [InlineData("photo.bmp", false)]
    public void Image_extension_is_validated(string fileName, bool expected)
    {
        var result = _validator.Validate(new BannerUpsertRequest { Title = "Ok", Image = File(fileName, 10) });
        Assert.Equal(expected, result.IsValid);
    }

    [Fact]
    public void Image_over_5mb_is_invalid()
    {
        var result = _validator.Validate(new BannerUpsertRequest
        {
            Title = "Ok",
            Image = File("big.png", 5 * 1024 * 1024 + 1)
        });
        Assert.False(result.IsValid);
    }
}
