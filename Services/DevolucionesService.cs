using System.Globalization;
using Biblioteca_Yerferson.Models;
using Dapper;

namespace Biblioteca_Yerferson.Services;

public sealed class DevolucionesService(DatabaseService database)
{
    public async Task<IEnumerable<Devolucion>> GetListAsync()
    {
        using var connection = database.CreateConnection();

        var rows = await connection.QueryAsync("""
            SELECT Id, PrestamoId, FechaDevolucion, Observacion
            FROM Devoluciones
            ORDER BY Id DESC;
            """);

        return rows.Select(row => new Devolucion(
            (int)row.Id,
            (int)row.PrestamoId,
            DateTime.Parse(
                (string)row.FechaDevolucion,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind),
            (string?)row.Observacion
        )).ToArray();
    }

    public async Task<Devolucion> SaveAsync(
        CrearDevolucion solicitud)
    {
        if (solicitud.PrestamoId <= 0)
            throw new ArgumentException(
                "El identificador del prestamo debe ser mayor que cero.");

        using var connection = database.CreateConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction(
            deferred: false);

        int existe = await connection.ExecuteScalarAsync<int>("""
            SELECT COUNT(*)
            FROM Prestamos
            WHERE Id = @Id;
            """,
            new { Id = solicitud.PrestamoId },
            transaction);

        if (existe == 0)
            throw new KeyNotFoundException(
                "El prestamo no existe.");

        int devuelto = await connection.ExecuteScalarAsync<int>("""
            SELECT COUNT(*)
            FROM Devoluciones
            WHERE PrestamoId = @Id;
            """,
            new { Id = solicitud.PrestamoId },
            transaction);

        if (devuelto > 0)
            throw new InvalidOperationException(
                "Este prestamo ya fue devuelto.");

        var fecha = DateTime.UtcNow;
        var observacion = solicitud.Observacion?.Trim();

        int id = await connection.ExecuteScalarAsync<int>("""
            INSERT INTO Devoluciones
                (PrestamoId, FechaDevolucion, Observacion)
            VALUES
                (@PrestamoId, @FechaDevolucion, @Observacion);

            SELECT last_insert_rowid();
            """,
            new
            {
                solicitud.PrestamoId,
                FechaDevolucion = fecha.ToString(
                    "O", CultureInfo.InvariantCulture),
                Observacion = observacion
            },
            transaction);

        transaction.Commit();

        return new Devolucion(
            id,
            solicitud.PrestamoId,
            fecha,
            observacion);
    }
}