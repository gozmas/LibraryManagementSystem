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

public BookController(IBookService bookService, IMapper mapper)
{
    _bookService = bookService;
    _mapper = mapper;
}

[Authorize]
[HttpGet("{id}")]
public async Task<ActionResult<BookListDto>> GetBook(int id)
{
    var book = await _bookService.GetByIdAsync(id);

    if (book == null)
    {
        return NotFound();
    }

    var bookDto = _mapper.Map<BookListDto>(book);

    return Ok(bookDto);
}

[Authorize(Roles = "Admin")]
[HttpPost]
public async Task<ActionResult<Book>> CreateBook(CreateBookDto createBookDto)
{
    var book = _mapper.Map<Book>(createBookDto);

    await _bookService.AddAsync(book);

    return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
}

[Authorize(Roles = "Admin")]
[HttpPut("{id}")]
public async Task<IActionResult> UpdateBook(int id, UpdateBookDto updateBookDto)
{
    var book = await _bookService.GetByIdAsync(id);

    if (book == null)
    {
        return NotFound();
    }

    _mapper.Map(updateBookDto, book);

    await _bookService.UpdateAsync(book);

    return NoContent();
}

[Authorize]
[HttpGet("search")]
public async Task<IActionResult> SearchBooks([FromQuery] BookQueryDto query)
{
    var result = await _bookService.SearchBooksAsync(query);
    return Ok(result);
}
}