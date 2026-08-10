using FluentValidation;
using Kela.Application.Features.Contents.Requests;

namespace Kela.Application.Features.Contents.Validators;

internal sealed class UpdateContentValidator : AbstractValidator<UpdateContentRequest>
{
    public UpdateContentValidator()
    {
        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Başlık zorunludur.")
            .Must(x => x.Trim().Length >= 2).WithMessage("Başlık en az 2 karakter olmalıdır.");

        RuleFor(x => x.Url)
            .Must(u => string.IsNullOrWhiteSpace(u) || u.Trim().Length <= 500)
            .WithMessage("URL en fazla 500 karakter olabilir.");
    }
}
