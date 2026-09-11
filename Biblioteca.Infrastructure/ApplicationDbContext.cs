using Biblioteca.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<IdentityUser> //EF Core administra los libros y las tablas de usuarios de Identity.
{
    public ApplicationDbContext (
        DbContextOptions<ApplicationDbContext> options) //recibe la configuración del contexto.
        : base(options) //entrega esa configuración al constructor de DbContext.
    {

    }

    public DbSet<Book> Books => Set<Book>();//es una forma corta de escribir una propiedad que devuelve el conjunto de entidades Book del contexto.
}
