using Dapper;
using Microsoft.Data.Sqlite;

namespace Biblioteca_Yerferson.Services;

public sealed class DatabaseService(
    IConfiguration configuration,
    IWebHostEnvironment environment)
{
    public SqliteConnection CreateConnection()
    {
        var connectionString =
            configuration.GetConnectionString("BibliotecaConnection")
            ?? throw new InvalidOperationException(
                "Falta configurar BibliotecaConnection.");

        var settings = new SqliteConnectionStringBuilder(
            connectionString)
        {
            ForeignKeys = true
        };

        if (!Path.IsPathRooted(settings.DataSource))
            settings.DataSource = Path.Combine(
                environment.ContentRootPath,
                settings.DataSource);

        var directory = Path.GetDirectoryName(settings.DataSource);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        return new SqliteConnection(settings.ToString());
    }

    public async Task InitializeAsync()
    {
        const string query = """
            CREATE TABLE IF NOT EXISTS Estudiantes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Apellido TEXT NOT NULL,
                Matricula TEXT NOT NULL COLLATE NOCASE UNIQUE,
                Celular TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Libros (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Titulo TEXT NOT NULL,
                Autor TEXT NOT NULL,
                Isbn TEXT NOT NULL UNIQUE,
                CantidadTotal INTEGER NOT NULL
                    CHECK (CantidadTotal >= 0)
            );

            CREATE TABLE IF NOT EXISTS Prestamos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EstudianteId INTEGER NOT NULL,
                LibroId INTEGER NOT NULL,
                FechaPrestamo TEXT NOT NULL,
                FechaLimite TEXT NOT NULL,

                FOREIGN KEY (EstudianteId)
                    REFERENCES Estudiantes (Id)
                    ON DELETE RESTRICT,

                FOREIGN KEY (LibroId)
                    REFERENCES Libros (Id)
                    ON DELETE RESTRICT
            );

            CREATE TABLE IF NOT EXISTS Devoluciones (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PrestamoId INTEGER NOT NULL UNIQUE,
                FechaDevolucion TEXT NOT NULL,
                Observacion TEXT NULL,

                FOREIGN KEY (PrestamoId)
                    REFERENCES Prestamos (Id)
                    ON DELETE RESTRICT
            );

            CREATE INDEX IF NOT EXISTS IX_Prestamos_EstudianteId
                ON Prestamos (EstudianteId);

            CREATE INDEX IF NOT EXISTS IX_Prestamos_LibroId
                ON Prestamos (LibroId);
            """;

        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            query,
            transaction: transaction);

        transaction.Commit();
    }
}