using Biblioteca.Application.UseCases.Books;
using Biblioteca.Application.UseCases.Books.DTOs;
using Biblioteca.Domain;
using Biblioteca.Domain.Repositories;
using Xunit; 


namespace Biblioteca.Application.Tests;

public class CreateBookUseCaseTest
{
    [Fact] // indica a xUnit que ese método es una prueba.
    public async Task ExecuteAsync_WithValidData_SavesBookAndReturnId()
    {
        var repository = new FakeBookRepository();
        var useCase = new CreateBookUseCase(repository);

        var dto = new CreateBookDto
        {
            Title = "Cien años de soledad",
            Author = "Gabriel Garcia Marquez",
            Isbn = "9788497592208",
            PublicationYear = 1967
        };

        int result = await useCase.ExecuteAsync(dto);

        Assert.Equal(42, result); //Cada Assert expresa algo que debe cumplirse. Por ejemplo, Assert.Equal(42, result) falla si el resultado es distinto de 42.
        Assert.Equal(1, repository.InsertCalls);
        Assert.NotNull(repository.SavedBook);
        Assert.Equal(dto.Title, repository.SavedBook.Title);
        Assert.Equal(dto.Author, repository.SavedBook.Author);
        Assert.Equal(dto.Isbn, repository.SavedBook.Isbn);
        Assert.Equal(dto.PublicationYear, repository.SavedBook.PublicationYear);

    }
    [Theory] //permite ejecutar una misma prueba con distintos datos.
    [InlineData("")] //proporciona cada entrada: primero "" y después "   ".
    [InlineData("   ")]
    public async Task ExecuteAsync_WithBlankTitle_ThrowsAndDoesNotSave(
    string title)
    {
        // Preparar
        var repository = new FakeBookRepository();
        var useCase = new CreateBookUseCase(repository);

        var dto = new CreateBookDto
        {
            Title = title,
            Author = "Gabriel García Márquez",
            Isbn = "9788497592208",
            PublicationYear = 1967
        };

        // Ejecutar y comprobar
        await Assert.ThrowsAsync<ArgumentException>( //comprueba que la operación rechaza el dato lanzando esa excepción. También verificamos que no intentó guardar el libro.
            () => useCase.ExecuteAsync(dto)); //entrega a xUnit la operación que debe ejecutar y observar.

        Assert.Equal(0, repository.InsertCalls);
    }
    private class FakeBookRepository : IBookRepository /*El FakeBookRepository implementa nuestra interfaz, 
                                              pero mantiene el libro en una propiedad en lugar de 
                                             enviarlo a SQL Server. Elegimos 42 como identificador de prueba: 
                                       así sabemos exactamente qué resultado debe devolver el caso de uso. */
    {
        public Book? SavedBook { get; private set; }

        public int InsertCalls { get; private set; }

        public Task<int> InsertAsync(Book book)
        {
            SavedBook = book;
            InsertCalls++;

            return Task.FromResult(42);
        }

        public Task<Book?> GetByIdAsync(int id) //Implementamos también GetByIdAsync porque la interfaz lo exige. Como esta prueba no debe consultar libros, lanzamos una excepción si alguien intenta utilizarlo.
        {
            throw new NotSupportedException(
                "Esta prueba no utiliza la búsqueda de libros.");
        }
        public Task<List<Book>> GetAllAsync()
        {
            throw new NotSupportedException(
                "Esta prueba no utiliza el listado de libros.");
        }
        public Task<bool> DeleteAsync(int id)
        {
            throw new NotSupportedException(
                "Esta prueba no utiliza la eliminación de libros.");
        }
        public Task<bool> UpdateAsync(Book book)
        {
            throw new NotSupportedException(
                "Esta prueba no utiliza la actualización de libros.");
        }
    }
}
/*zknnxzSegún ese resultado, la primera prueba pasó: el caso de uso envía los datos al repositorio y devuelve su identificador.
Ahora añadiremos una regla: no se puede crear un libro con el título vacío o compuesto solo por espacios.*/