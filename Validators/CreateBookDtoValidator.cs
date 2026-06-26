using FluentValidation;
using LibraryManagementSystem.API.Dtos;

namespace LibraryManagementSystem.API.Validators;

public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Book title is required.")
            .MaximumLength(100).WithMessage("Book title cannot exceed 100 characters.");

        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required.")
            .Length(13).WithMessage("ISBN must be exactly 13 characters.");

        RuleFor(x => x.PublicationYear)
            .InclusiveBetween(1450, DateTime.Now.Year)
            .WithMessage("Publication year must be valid.");

        RuleFor(x => x.AuthorId)
            .GreaterThan(0).WithMessage("AuthorId must be greater than 0.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId must be greater than 0.");
    }
}