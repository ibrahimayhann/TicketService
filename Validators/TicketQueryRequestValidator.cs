using FluentValidation;
using TicketApi.DTOs.Request;

namespace TicketApi.Validators;

public sealed class TicketQueryRequestValidator : AbstractValidator<TicketQueryRequest>
{
    public TicketQueryRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        RuleFor(x => x.Sort!)
            .Must(s => new[] {
                "createdAtDesc", "createdAtAsc",
                "updatedAtDesc", "updatedAtAsc"
            }.Contains(s))
            .When(x => !string.IsNullOrWhiteSpace(x.Sort));
    }
}
    