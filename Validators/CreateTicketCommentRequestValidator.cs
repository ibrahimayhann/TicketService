using FluentValidation;
using TicketApi.DTOs.Request;

namespace TicketApi.Validators;

public sealed class CreateTicketCommentRequestValidator : AbstractValidator<CreateTicketCommentRequest>
{
    public CreateTicketCommentRequestValidator()
    {
        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(80).WithMessage("Author can be max 80 characters.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(500).WithMessage("Message can be max 500 characters.");
    }
}
