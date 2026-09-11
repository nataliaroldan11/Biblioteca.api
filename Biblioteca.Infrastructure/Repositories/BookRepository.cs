using Biblioteca.Domain;
using Biblioteca.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Infrastructure.Repositories;

public class BookRepository : IBookRepository //implementa el contrato ya creado
{
    private readonly ApplicationDbContext _context;

    public BookRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> InsertAsync(Book book)
    {
        _context.Books.Add(book);

        await _context.SaveChangesAsync();

        return book.Id;
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(book => book.Id == id);
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _context.Books
            .AsNoTracking()
            .OrderBy(book => book.Id)
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book is null)
        {
            return false;
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<bool> UpdateAsync(Book book)
    {
        var existingBook = await _context.Books.FindAsync(book.Id);

        if (existingBook is null)
        {
            return false;
        }

        existingBook.Title = book.Title;
        existingBook.Author = book.Author;
        existingBook.Isbn = book.Isbn;
        existingBook.PublicationYear = book.PublicationYear;

        await _context.SaveChangesAsync();

        return true;
    }
}
