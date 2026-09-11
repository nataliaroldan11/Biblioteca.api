

using Biblioteca.Application.UseCases.Books;
using Biblioteca.Application.UseCases.Books.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Biblioteca.Api.Controllers;



[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly CreateBookUseCase _createBook;
    private readonly GetBookByIdUseCase _getBookById;
    private readonly GetAllBooksUseCase _getAllBooks;
    private readonly DeleteBookUseCase _deleteBook;
    private readonly UpdateBookUseCase _updateBook;

    public BooksController(
     CreateBookUseCase createBook,
     GetBookByIdUseCase getBookById,
     GetAllBooksUseCase getAllBooks,
     DeleteBookUseCase deleteBook,
     UpdateBookUseCase updateBook)
    {
        _createBook = createBook;
        _getBookById = getBookById;
        _getAllBooks = getAllBooks;
        _deleteBook = deleteBook;
        _updateBook = updateBook;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var book = await _getBookById.ExecuteAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        return Ok(book);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateBookDto dto)
    {
        try
        {
            int id = await _createBook.ExecuteAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                new { id });
        }
        catch (ArgumentException exception)
        {
            return Problem(
                title: "Datos inválidos",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var books = await _getAllBooks.ExecuteAsync();

        return Ok(books);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await _deleteBook.ExecuteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdateBookDto dto)
    {
        try
        {
            bool updated = await _updateBook.ExecuteAsync(id, dto);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return Problem(
                title: "Datos inválidos",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
