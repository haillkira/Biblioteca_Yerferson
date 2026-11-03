namespace Biblioteca_Yerferson.Models;

public sealed record Libro(
    int Id,
    string Titulo,
    string Autor,
    string Isbn,
    int CantidadTotal);