using Kela.Core.Modules.User.Requests;
using Kela.Core.Modules.User.Validators;

namespace Kela.Tests.User;

public class UserValidatorTests
{
    private readonly UserUpsertRequestValidator _validator = new();

    [Theory]
    [InlineData("admin")]
    [InlineData("teacher")]
    public void Valid_type_passes(string type)
        => Assert.True(_validator.Validate(new UserUpsertRequest { Email = "a@b.com", Type = type }).IsValid);

    [Fact]
    public void Invalid_type_fails()
        => Assert.False(_validator.Validate(new UserUpsertRequest { Email = "a@b.com", Type = "student" }).IsValid);

    [Fact]
    public void Invalid_email_fails()
        => Assert.False(_validator.Validate(new UserUpsertRequest { Email = "not-an-email", Type = "teacher" }).IsValid);

    [Fact]
    public void Short_password_fails()
        => Assert.False(_validator
            .Validate(new UserUpsertRequest { Email = "a@b.com", Type = "teacher", Password = "123" })
            .IsValid);

    [Fact]
    public void Empty_password_is_allowed_for_update()
        => Assert.True(_validator
            .Validate(new UserUpsertRequest { Email = "a@b.com", Type = "teacher", Password = "" })
            .IsValid);
}
