using System;
using System.Collections.Generic;
using System.Text;
using Biblioteca.Domain.Repositories;

namespace Biblioteca.Application.UseCases.Books;

public class DeleteBookUseCase
{
    private readonly IBookRepository _bookRepository;

    public DeleteBookUseCase(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<bool> ExecuteAsync(int id)
    {
        return await _bookRepository.DeleteAsync(id);
    }
}
