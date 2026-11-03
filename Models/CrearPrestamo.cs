namespace Biblioteca_Yerferson.Models;

public sealed record CrearPrestamo(
    int EstudianteId,
    int LibroId,
    DateTime FechaLimite);