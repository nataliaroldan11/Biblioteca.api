using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Application.UseCases.Books.DTOs;

public class CreateBookDto //Datos que se tienen que proporcionar para crear el libro//
{
    //propiedades 
    public string Title { get; set; } = string.Empty;
    public string Author {  get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int PublicationYear { get; set; }

}
