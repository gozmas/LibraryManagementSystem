using LibraryManagementSystem.API.Responses;
using Microsoft.AspNetCore.Authorization;
using LibraryManagementSystem.API.Services.Interfaces;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace LibraryManagementSystem.API.Controllers;

[Route("api/books")]
[ApiController]
public class BookController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly IMapper _mapper;
    private readonly ILogger<BookController> _logger;

    public BookController(
        IBookService bookService,
        IMapper mapper,
        ILogger<BookController> logger)
    {
        _bookService = bookService;
        _mapper = mapper;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllBooks()
    {
        var books = await _bookService.GetAllAsync();

        var bookDtos = _mapper.Map<List<BookListDto>>(books);

        return Ok(bookDtos);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBook(int id)
    {
        var book = await _bookService.GetByIdAsync(id);

        if (book == null)
        {
            _logger.LogWarning("Book with ID {BookId} was not found.", id);
            return NotFound();
        }

        var bookDto = _mapper.Map<BookListDto>(book);

        return Ok(new ApiResponse<BookListDto>(
            true,
            "Book retrieved successfully.",
            bookDto));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Book>> CreateBook(CreateBookDto createBookDto)
    {
        var book = _mapper.Map<Book>(createBookDto);

        await _bookService.AddAsync(book);

        _logger.LogInformation("Book '{Title}' was created successfully.", book.Title);

        var createdBook = await _bookService.GetByIdAsync(book.Id);

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, createdBook);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(int id, UpdateBookDto updateBookDto)
    {
        var book = await _bookService.GetByIdAsync(id);

        if (book == null)
        {
            return NotFound(new ApiResponse<object>(
                false,
                "Book not found.",
                null));
        }

        var oldTotalCopies = book.TotalCopies;

        _mapper.Map(updateBookDto, book);

        var copiesDelta = book.TotalCopies - oldTotalCopies;
        var newAvailableCopies = book.AvailableCopies + copiesDelta;

        if (newAvailableCopies < 0)
            newAvailableCopies = 0;

        if (newAvailableCopies > book.TotalCopies)
            newAvailableCopies = book.TotalCopies;

        book.AvailableCopies = newAvailableCopies;
        book.IsAvailable = book.AvailableCopies > 0;

        await _bookService.UpdateAsync(book);

        _logger.LogInformation("Book with ID {BookId} was updated successfully.", id);

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _bookService.GetByIdAsync(id);

        if (book == null)
        {
            return NotFound(new ApiResponse<object>(
                false,
                "Book not found.",
                null));
        }

        if (book.AvailableCopies < book.TotalCopies)
        {
            return BadRequest(new ApiResponse<object>(
                false,
                "Some copies of this book are currently borrowed and cannot be deleted.",
                null));
        }

        try
        {
            await _bookService.DeleteAsync(book);

            _logger.LogInformation("Book with ID {BookId} was deleted successfully.", id);

            return Ok(new ApiResponse<object>(
                true,
                "Book deleted successfully.",
                null));
        }
        catch (DbUpdateException)
        {
            _logger.LogWarning(
                "Book delete failed because the book has related loan records. BookId: {BookId}",
                id);

            return BadRequest(new ApiResponse<object>(
                false,
                "This book has loan records and cannot be deleted.",
                null));
        }
    }

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<IActionResult> SearchBooks([FromQuery] BookQueryDto query)
    {
        var result = await _bookService.SearchBooksAsync(query);

        _logger.LogInformation(
            "Book search executed. SearchTerm: {SearchTerm}, PageNumber: {PageNumber}, PageSize: {PageSize}, SortBy: {SortBy}, SortDirection: {SortDirection}",
            query.SearchTerm,
            query.PageNumber,
            query.PageSize,
            query.SortBy,
            query.SortDirection);

        return Ok(new ApiResponse<PagedResultDto<BookListDto>>(
            true,
            "Books retrieved successfully.",
            result));
    }
}