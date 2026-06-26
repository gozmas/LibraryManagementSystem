using FluentValidation;
using LibraryManagementSystem.API.Dtos;

namespace LibraryManagementSystem.API.Validators;

public class ReturnBookDtoValidator : AbstractValidator<ReturnBookDto>
{
    public ReturnBookDtoValidator()
    {
        RuleFor(x => x.LoanId)
            .GreaterThan(0).WithMessage("LoanId must be greater than 0.");
    }
}