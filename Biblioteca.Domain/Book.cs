namespace Biblioteca.Domain;

public class Book
{
    public int Id { get; set; } 
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty; //texto ya que es un identificador, no cantidad.// 
    public int PublicationYear { get; set; }

}
