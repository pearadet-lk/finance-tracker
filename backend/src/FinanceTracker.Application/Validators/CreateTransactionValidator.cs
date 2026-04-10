using FinanceTracker.Application.Commands.Transactions;
using FluentValidation;

namespace FinanceTracker.Application.Validators;

public class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(1_000_000).WithMessage("Amount cannot exceed 1,000,000");

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => t is "Income" or "Expense")
            .WithMessage("Type must be either 'Income' or 'Expense'");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Date)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Date cannot be in the future");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }
}
