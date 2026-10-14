using Kela.Core.Modules.Group.Requests;
using Kela.Core.Modules.Group.Validators;

namespace Kela.Tests.Group;

public class GroupValidatorTests
{
    private readonly GroupUpsertRequestValidator _validator = new();

    [Fact]
    public void Empty_name_is_invalid()
        => Assert.False(_validator.Validate(new GroupUpsertRequest { Name = "" }).IsValid);

    [Fact]
    public void Name_longer_than_190_is_invalid()
        => Assert.False(_validator.Validate(new GroupUpsertRequest { Name = new string('a', 191) }).IsValid);

    [Fact]
    public void Description_longer_than_255_is_invalid()
        => Assert.False(_validator.Validate(new GroupUpsertRequest { Name = "G", Description = new string('a', 256) }).IsValid);

    [Fact]
    public void Valid_request_passes()
        => Assert.True(_validator.Validate(new GroupUpsertRequest { Name = "Group A", Description = "desc" }).IsValid);
}
