using FluentValidation;
using InstitutoBlazor.Data;
using InstitutoBlazor.Models;
using Microsoft.EntityFrameworkCore;

namespace InstitutoBlazor.Validators;

/// <summary>Reglas de negocio asíncronas complementarias a las Data Annotations del modelo.</summary>
public class EmpleadoValidator : AbstractValidator<Empleado>
{
    public EmpleadoValidator(IDbContextFactory<AppDbContext> factory)
    {
        RuleFor(e => e.Dni).MustAsync(async (empleado, dni, ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            return !await db.Empleados.AnyAsync(e => e.Dni == dni.Trim() && e.Id != empleado.Id, ct);
        }).WithMessage("El DNI ya está registrado en otro empleado.");

        RuleFor(e => e.IdDepartamento).MustAsync(async (id, ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            return await db.Departamentos.AnyAsync(d => d.IdDepartamento == id, ct);
        }).WithMessage("Seleccione un departamento existente.");

        RuleFor(e => e.CodPostal).MustAsync(async (codigo, ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            return await db.Localidades.AnyAsync(l => l.CodPostal == codigo, ct);
        }).WithMessage("Seleccione una localidad existente.");
    }
}
