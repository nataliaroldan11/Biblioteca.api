using System;
using System.Collections.Generic;
using System.Text;
using Biblioteca.Domain;

namespace Biblioteca.Domain.Repositories;

public interface IBookRepository
{
    Task<int> InsertAsync(Book book);
    Task<Book?> GetByIdAsync(int id);
    Task<List<Book>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateAsync(Book book); 
}

//interface define un contrato que otra clase debera implementar
