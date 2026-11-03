using Biblioteca_Yerferson.Models;
using Dapper;

namespace Biblioteca_Yerferson.Services;

public sealed class LibrosService(DatabaseService database)
{
    public async Task<IEnumerable<Libro>> GetListAsync()
    {
        using var connection = database.CreateConnection();

        var rows = await connection.QueryAsync("""
            SELECT Id, Titulo, Autor, Isbn, CantidadTotal
            FROM Libros
            ORDER BY Id;
            """);

        return rows.Select(row => new Libro(
            (int)row.Id,
            (string)row.Titulo,
            (string)row.Autor,
            (string)row.Isbn,
            (int)row.CantidadTotal
        )).ToArray();
    }

    public async Task<Libro> SaveAsync(Libro libro)
    {
        if (string.IsNullOrWhiteSpace(libro.Titulo)
            || string.IsNullOrWhiteSpace(libro.Autor)
            || string.IsNullOrWhiteSpace(libro.Isbn))
            throw new ArgumentException(
                "Titulo, autor e ISBN son obligatorios.");

        if (libro.CantidadTotal < 0)
            throw new ArgumentException(
                "La cantidad total no puede ser negativa.");

        var nuevo = libro with
        {
            Titulo = libro.Titulo.Trim(),
            Autor = libro.Autor.Trim(),
            Isbn = libro.Isbn.Trim()
        };

        using var connection = database.CreateConnection();

        int id = await connection.ExecuteScalarAsync<int>("""
            INSERT INTO Libros (Titulo, Autor, Isbn, CantidadTotal)
            VALUES (@Titulo, @Autor, @Isbn, @CantidadTotal);

            SELECT last_insert_rowid();
            """, nuevo);

        return nuevo with { Id = id };
    }

    public async Task<int> GetDisponiblesAsync(int id)
    {
        using var connection = database.CreateConnection();

        int? cantidad = await connection.QuerySingleOrDefaultAsync<int?>("""
            SELECT CantidadTotal
            FROM Libros
            WHERE Id = @Id;
            """, new { Id = id });

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
            """, new { Id = id });

        return cantidad.Value - pendientes;
    }
}