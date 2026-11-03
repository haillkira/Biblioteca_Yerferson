using System.Globalization;
using Biblioteca_Yerferson.Models;
using Dapper;

namespace Biblioteca_Yerferson.Services;

public sealed class PrestamosService(DatabaseService database)
{
    public async Task<IEnumerable<Prestamo>> GetListAsync()
    {
        using var connection = database.CreateConnection();

        var rows = await connection.QueryAsync("""
            SELECT Id, EstudianteId, LibroId,
                   FechaPrestamo, FechaLimite
            FROM Prestamos
            ORDER BY Id DESC;
            """);

        return rows.Select(row => new Prestamo(
            (int)row.Id,
            (int)row.EstudianteId,
            (int)row.LibroId,
            ReadDate((string)row.FechaPrestamo),
            ReadDate((string)row.FechaLimite)
        )).ToArray();
    }

    public async Task<Prestamo> SaveAsync(CrearPrestamo solicitud)
    {
        if (solicitud.EstudianteId <= 0 || solicitud.LibroId <= 0)
            throw new ArgumentException(
                "Los identificadores deben ser mayores que cero.");

        var ahora = DateTime.UtcNow;

        var limite = solicitud.FechaLimite.Kind == DateTimeKind.Local
            ? solicitud.FechaLimite.ToUniversalTime()
            : DateTime.SpecifyKind(
                solicitud.FechaLimite,
                DateTimeKind.Utc);

        if (limite <= ahora)
            throw new ArgumentException(
                "La fecha limite debe ser posterior al momento actual.");

        using var connection = database.CreateConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction(
            deferred: false);

        int estudianteExiste = await connection.ExecuteScalarAsync<int>("""
            SELECT COUNT(*)
            FROM Estudiantes
            WHERE Id = @Id;
            """,
            new { Id = solicitud.EstudianteId },
            transaction);

        if (estudianteExiste == 0)
            throw new KeyNotFoundException(
                "El estudiante no existe.");

        int? cantidad = await connection.QuerySingleOrDefaultAsync<int?>("""
            SELECT CantidadTotal
            FROM Libros
            WHERE Id = @Id;
            """,
            new { Id = solicitud.LibroId },
            transaction);

        if (cantidad is null)
            throw new KeyNotFoundException("El libro no existe.");

        int pendientes = await connection.ExecuteScalarAsync<int>("""
            SELECT COUNT(*)
            FROM Prestamos p
            WHERE p.LibroId = @Id
              AND NOT EXISTS (
                  SELECT 1
                  FROM Devoluciones d
                  WHERE d.PrestamoId = p.Id
              );
            """,
            new { Id = solicitud.LibroId },
            transaction);

        if (pendientes >= cantidad.Value)
            throw new InvalidOperationException(
                "No quedan ejemplares disponibles.");

        int id = await connection.ExecuteScalarAsync<int>("""
            INSERT INTO Prestamos
                (EstudianteId, LibroId, FechaPrestamo, FechaLimite)
            VALUES
                (@EstudianteId, @LibroId, @FechaPrestamo, @FechaLimite);

            SELECT last_insert_rowid();
            """,
            new
            {
                solicitud.EstudianteId,
                solicitud.LibroId,
                FechaPrestamo = ahora.ToString(
                    "O", CultureInfo.InvariantCulture),
                FechaLimite = limite.ToString(
                    "O", CultureInfo.InvariantCulture)
            },
            transaction);

        transaction.Commit();

        return new Prestamo(
            id,
            solicitud.EstudianteId,
            solicitud.LibroId,
            ahora,
            limite);
    }

    private static DateTime ReadDate(string value) =>
        DateTime.Parse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);
}