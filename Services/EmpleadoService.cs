using System.ComponentModel.DataAnnotations;
using InstitutoBlazor.Data;
using InstitutoBlazor.Models;
using Microsoft.EntityFrameworkCore;

namespace InstitutoBlazor.Services;

/// <inheritdoc cref="IEmpleadoService" />
public class EmpleadoService(
    IDbContextFactory<AppDbContext> dbFactory,
    FluentValidation.IValidator<Empleado> validator,
    ILogger<EmpleadoService> logger) : IEmpleadoService
{
    /// <inheritdoc />
    public async Task<PagedResult<Empleado>> ObtenerPaginadoAsync(string? texto = null, int departamento = 0, int pagina = 1, int tamanio = 10)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var query = db.Empleados.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(texto))
        {
            texto = texto.Trim();
            query = query.Where(e => e.Dni.Contains(texto) || e.Nombre.Contains(texto) || e.Apellido.Contains(texto));
        }
        if (departamento > 0) query = query.Where(e => e.IdDepartamento == departamento);

        tamanio = Math.Clamp(tamanio, 1, 50);
        var total = await query.CountAsync();
        pagina = Math.Clamp(pagina, 1, Math.Max(1, (int)Math.Ceiling(total / (double)tamanio)));
        var items = await query.Include(e => e.Departamento).Include(e => e.Localidad)
            .OrderBy(e => e.Apellido).ThenBy(e => e.Nombre).ThenBy(e => e.Id)
            .Skip((pagina - 1) * tamanio).Take(tamanio).ToListAsync();
        logger.LogInformation("Consulta de empleados: página {Pagina}, tamaño {Tamanio}, total {Total}", pagina, tamanio, total);
        return new(items, total, pagina, tamanio);
    }

    /// <inheritdoc />
    public async Task<Empleado?> ObtenerPorIdAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Empleados.Include(e => e.Departamento).Include(e => e.Localidad)
            .AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <inheritdoc />
    public async Task<(int Empleados, int Departamentos)> ObtenerEstadisticasAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return (await db.Empleados.CountAsync(), await db.Empleados.Select(e => e.IdDepartamento).Distinct().CountAsync());
    }

    /// <inheritdoc />
    public Task<(bool Ok, string Mensaje)> CrearAsync(Empleado empleado) => GuardarAsync(empleado, nuevo: true);

    /// <inheritdoc />
    public Task<(bool Ok, string Mensaje)> ActualizarAsync(Empleado empleado) => GuardarAsync(empleado, nuevo: false);

    private async Task<(bool Ok, string Mensaje)> GuardarAsync(Empleado empleado, bool nuevo)
    {
        empleado.Dni = empleado.Dni.Trim();
        empleado.Nombre = empleado.Nombre.Trim();
        empleado.Apellido = empleado.Apellido.Trim();
        empleado.Telefono = empleado.Telefono?.Trim();
        empleado.Direccion = empleado.Direccion?.Trim();
        var errores = new List<ValidationResult>();
        if (!Validator.TryValidateObject(empleado, new ValidationContext(empleado), errores, true))
            return (false, string.Join(" ", errores.Select(e => e.ErrorMessage)));

        var validacion = await validator.ValidateAsync(empleado);
        if (!validacion.IsValid) return (false, string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));

        await using var db = await dbFactory.CreateDbContextAsync();
        var actual = nuevo ? new Empleado() : await db.Empleados.FindAsync(empleado.Id);
        if (actual is null) return (false, "El empleado ya no existe. Volvé al listado.");
        actual.Dni = empleado.Dni;
        actual.Nombre = empleado.Nombre;
        actual.Apellido = empleado.Apellido;
        actual.Telefono = empleado.Telefono;
        actual.Direccion = empleado.Direccion;
        actual.IdDepartamento = empleado.IdDepartamento;
        actual.CodPostal = empleado.CodPostal;
        if (nuevo) db.Empleados.Add(actual);
        try
        {
            await db.SaveChangesAsync();
            empleado.Id = actual.Id;
            logger.LogInformation("Empleado {EmpleadoId}: operación {Operacion}", actual.Id, nuevo ? "alta" : "modificación");
            return (true, nuevo ? "Empleado guardado correctamente." : "Empleado actualizado correctamente.");
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "No se pudo guardar el empleado {EmpleadoId}", empleado.Id);
            return (false, "No se pudo guardar. El DNI puede haberse registrado mientras editabas; revisá los datos y reintentá.");
        }
    }

    /// <inheritdoc />
    public async Task<(bool Ok, string Mensaje)> EliminarAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var empleado = await db.Empleados.FindAsync(id);
        if (empleado is null) return (false, "El empleado ya no existe.");
        db.Empleados.Remove(empleado);
        try
        {
            await db.SaveChangesAsync();
            logger.LogInformation("Empleado {EmpleadoId} eliminado", id);
            return (true, "Registro eliminado correctamente.");
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "No se pudo eliminar el empleado {EmpleadoId}", id);
            return (false, "No se pudo eliminar el registro. Actualizá el listado y reintentá.");
        }
    }
}
