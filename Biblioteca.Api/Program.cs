
using Biblioteca.Application.UseCases.Books;
using Biblioteca.Api.Authentication;
using Biblioteca.Domain.Repositories;
using Biblioteca.Infrastructure;
using Biblioteca.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        //builder.Services.AddOpenApi();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("Biblioteca")));

        // Identity administra usuarios y hashes de contraseñas mediante EF Core.
        builder.Services
            .AddIdentityCore<IdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddLibraryAuthentication();

        builder.Services.AddScoped<IBookRepository, BookRepository>(); //ndica: “Cuando alguien necesite IBookRepository, entrégale BookRepository”. Scoped hace que esa instancia se comparta dentro de una petición HTTP.
        builder.Services.AddScoped<CreateBookUseCase>();
        builder.Services.AddScoped<GetBookByIdUseCase>();
        builder.Services.AddScoped<GetAllBooksUseCase>();
        builder.Services.AddScoped<DeleteBookUseCase>();
        builder.Services.AddScoped<UpdateBookUseCase>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            //app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
