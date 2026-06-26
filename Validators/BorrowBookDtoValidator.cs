using FluentValidation;
using LibraryManagementSystem.API.Dtos;

namespace LibraryManagementSystem.API.Validators;

public class BorrowBookDtoValidator : AbstractValidator<BorrowBookDto>
{
    public BorrowBookDtoValidator()
    {
        RuleFor(x => x.BookId)
            .GreaterThan(0).WithMessage("BookId must be greater than 0.");

        RuleFor(x => x.MemberId)
            .GreaterThan(0).WithMessage("MemberId must be greater than 0.");
    }
}