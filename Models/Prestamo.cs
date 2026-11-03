namespace Biblioteca_Yerferson.Models;

public sealed record Prestamo(
    int Id,
    int EstudianteId,
    int LibroId,
    DateTime FechaPrestamo,
    DateTime FechaLimite);