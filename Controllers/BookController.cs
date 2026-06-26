using LibraryManagementSystem.API.Responses;
using Microsoft.AspNetCore.Authorization;
using LibraryManagementSystem.API.Services.Interfaces;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
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

[Authorize]
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

    return Ok(
    new ApiResponse<BookListDto>(
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

    return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
}

[Authorize(Roles = "Admin")]
[HttpPut("{id}")]
public async Task<IActionResult> UpdateBook(int id, UpdateBookDto updateBookDto)
{
    var book = await _bookService.GetByIdAsync(id);

    if (book == null)
    {
        return NotFound(
    new ApiResponse<object>(
        false,
        "Book not found.",
        null));
    }

    _mapper.Map(updateBookDto, book);

    await _bookService.UpdateAsync(book);
   _logger.LogInformation("Book with ID {BookId} was updated successfully.", id);

    return NoContent();
}

[Authorize]
[HttpGet("search")]
public async Task<IActionResult> SearchBooks([FromQuery] BookQueryDto query)
{
    var result = await _bookService.SearchBooksAsync(query);
    _logger.LogInformation("Book search executed.");
    return Ok(result);
}
}