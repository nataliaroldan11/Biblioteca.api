using Biblioteca.Application.UseCases.Books.DTOs;
using Biblioteca.Domain;
using Biblioteca.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Application.UseCases.Books;
public class CreateBookUseCase
{
    private readonly IBookRepository _bookRepository; //Guarda la referencia al repositorio. private limita su acceso a esta clase y readonly impide reemplazar esa referencia 

    public CreateBookUseCase(IBookRepository bookRepository) //recibe el repositorio desde afuera mediante inyeccion de dependencias en el contructor, la clase recibe lo que necesita para trabajar
    {
        _bookRepository = bookRepository; 
    }

    public async Task<int> ExecuteAsync(CreateBookDto dto) //ejecuta la operacion, contruye la entidad del DTO y vuelve el id obtenido
    {
        if (string.IsNullOrWhiteSpace(dto.Title)) //detecta si el título es null, está vacío o contiene únicamente espacios en blanco.
        {
            throw new ArgumentException("El título es obligatorio."); //interrumpe la operación lanzando una excepción. Por eso, cuando el título es inválido, no llegamos a llamar al repositorio.
        }

        var book = new Book
        {
            Title = dto.Title,
            Author = dto.Author,
            Isbn = dto.Isbn,
            PublicationYear = dto.PublicationYear

        };

        int id = await _bookRepository.InsertAsync(book);

        return id;
    }
}
