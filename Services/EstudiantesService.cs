using Biblioteca_Yerferson.Models;
using Dapper;

namespace Biblioteca_Yerferson.Services;

public sealed class EstudiantesService(DatabaseService database)
{
    public async Task<IEnumerable<Estudiante>> GetListAsync()
    {
        using var connection = database.CreateConnection();

        var rows = await connection.QueryAsync("""
            SELECT Id, Nombre, Apellido, Matricula, Celular
            FROM Estudiantes
            ORDER BY Id;
            """);

        return rows.Select(row => new Estudiante(
            (int)row.Id,
            (string)row.Nombre,
            (string)row.Apellido,
            (string)row.Matricula,
            (string)row.Celular
        )).ToArray();
    }

    public async Task<Estudiante> SaveAsync(Estudiante estudiante)
    {
        if (string.IsNullOrWhiteSpace(estudiante.Nombre)
            || string.IsNullOrWhiteSpace(estudiante.Apellido)
            || string.IsNullOrWhiteSpace(estudiante.Matricula)
            || string.IsNullOrWhiteSpace(estudiante.Celular))
            throw new ArgumentException(
                "Nombre, apellido, matricula y celular son obligatorios.");

        var nuevo = estudiante with
        {
            Nombre = estudiante.Nombre.Trim(),
            Apellido = estudiante.Apellido.Trim(),
            Matricula = estudiante.Matricula.Trim(),
            Celular = estudiante.Celular.Trim()
        };

        using var connection = database.CreateConnection();

        int id = await connection.ExecuteScalarAsync<int>("""
            INSERT INTO Estudiantes
                (Nombre, Apellido, Matricula, Celular)
            VALUES
                (@Nombre, @Apellido, @Matricula, @Celular);

            SELECT last_insert_rowid();
            """, nuevo);

        return nuevo with { Id = id };
    }
}