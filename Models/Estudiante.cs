namespace Biblioteca_Yerferson.Models;

public sealed record Estudiante(
    int Id,
    string Nombre,
    string Apellido,
    string Matricula,
    string Celular);