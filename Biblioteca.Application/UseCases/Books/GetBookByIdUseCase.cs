using Biblioteca.Domain;
using Biblioteca.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Application.UseCases.Books;

public class GetBookByIdUseCase
{
    private readonly IBookRepository _bookRepository;

    public GetBookByIdUseCase(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<Book?> ExecuteAsync(int id)
    {
        return await _bookRepository.GetByIdAsync(id);
    }
}
