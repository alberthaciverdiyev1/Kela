using FluentValidation;
using Kela.Application.Features.Questions.Requests;

namespace Kela.Application.Features.Questions.Validators;

internal sealed class UpdateQuestionValidator : AbstractValidator<UpdateQuestionRequest>
{
    public UpdateQuestionValidator()
    {
        RuleFor(x => x.Text)
            .Cascade(CascadeMode.Stop)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Soru metni zorunludur.")
            .Must(x => x.Trim().Length >= 2).WithMessage("Soru metni en az 2 karakter olmalıdır.");

        RuleFor(x => x.OptionA)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("A şıkkı zorunludur.");

        RuleFor(x => x.OptionB)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("B şıkkı zorunludur.");

        RuleFor(x => x.OptionC)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("C şıkkı zorunludur.");

        RuleFor(x => x.OptionD)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Trim().Length <= 500)
            .WithMessage("D şıkkı en fazla 500 karakter olabilir.");

        RuleFor(x => x.OptionE)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Trim().Length <= 500)
            .WithMessage("E şıkkı en fazla 500 karakter olabilir.");

        RuleFor(x => x.CorrectOption)
            .InclusiveBetween(0, 4).WithMessage("Doğru cevap A ile E arasında olmalıdır.");

        RuleFor(x => x)
            .Must(x => x.CorrectOption switch
            {
                3 => !string.IsNullOrWhiteSpace(x.OptionD),
                4 => !string.IsNullOrWhiteSpace(x.OptionE),
                _ => true
            })
            .WithMessage("Doğru cevap boş şıkka işaret edemez.");
    }
}
