using System;
using System.Collections.Generic;
using System.Text;
using Biblioteca.Application.UseCases.Books.DTOs;
using Biblioteca.Domain;
using Biblioteca.Domain.Repositories;

namespace Biblioteca.Application.UseCases.Books;

public class UpdateBookUseCase
{
    private readonly IBookRepository _bookRepository;

    public UpdateBookUseCase(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<bool> ExecuteAsync(int id, UpdateBookDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("El título es obligatorio.");
        }

        var book = new Book
        {
            Id = id,
            Title = dto.Title,
            Author = dto.Author,
            Isbn = dto.Isbn,
            PublicationYear = dto.PublicationYear
        };

        return await _bookRepository.UpdateAsync(book);
    }
}