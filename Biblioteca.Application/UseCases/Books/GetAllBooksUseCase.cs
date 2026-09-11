using Biblioteca.Domain;
using Biblioteca.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Application.UseCases.Books;

public class GetAllBooksUseCase
{
    private readonly IBookRepository _bookRepository;

    public GetAllBooksUseCase(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<List<Book>> ExecuteAsync()
    {
        return await _bookRepository.GetAllAsync();
    }
}