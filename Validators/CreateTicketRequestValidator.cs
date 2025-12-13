using FluentValidation;
using TicketApi.DTOs.Request;

namespace TicketApi.Validators;

public sealed class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık eklemek zorunlu")
            .MaximumLength(150).WithMessage("Başlık 150 karakteri geçemez");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Açıklama eklemek zorunlu");

        RuleFor(x => x.Assignee)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Assignee));

        RuleFor(x => x.Tags)
            .MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.Tags));
    }
}
