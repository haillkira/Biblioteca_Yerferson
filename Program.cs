using Biblioteca_Yerferson.Services;
using Microsoft.Data.Sqlite;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddScoped<EstudiantesService>();
builder.Services.AddScoped<LibrosService>();
builder.Services.AddScoped<PrestamosService>();
builder.Services.AddScoped<DevolucionesService>();

var app = builder.Build();

await app.Services
    .GetRequiredService<DatabaseService>()
    .InitializeAsync();

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (ArgumentException exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(
            new { mensaje = exception.Message });
    }
    catch (KeyNotFoundException exception)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(
            new { mensaje = exception.Message });
    }
    catch (InvalidOperationException exception)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await context.Response.WriteAsJsonAsync(
            new { mensaje = exception.Message });
    }
    catch (SqliteException exception)
        when (exception.SqliteErrorCode == 19)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await context.Response.WriteAsJsonAsync(new
        {
            mensaje =
                "El registro esta duplicado o incumple una restriccion de la base de datos."
        });
    }
});

app.UseHttpsRedirection();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.Run();