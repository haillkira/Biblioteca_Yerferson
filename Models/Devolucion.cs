namespace Biblioteca_Yerferson.Models;

public sealed record Devolucion(
    int Id,
    int PrestamoId,
    DateTime FechaDevolucion,
    string? Observacion);