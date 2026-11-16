using InstitutoBlazor.Data;
using InstitutoBlazor.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InstitutoBlazor.Services;

public class UsuarioService(IDbContextFactory<AppDbContext> dbFactory, ILogger<UsuarioService> logger)
{
    private readonly PasswordHasher<Usuario> _hasher = new();

    public async Task<(bool Ok, string Mensaje, Usuario? Usuario)> ValidarAsync(string login, string password)
    {
        login = login.Trim();
        await using var db = await dbFactory.CreateDbContextAsync();
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Nombre == login || u.Email == login);
        if (usuario is null) return (false, "Usuario o contraseña incorrectos.", null);

        var result = _hasher.VerifyHashedPassword(usuario, usuario.Contrasenia, password);
        if (result == PasswordVerificationResult.Failed) return (false, "Usuario o contraseña incorrectos.", null);
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.Contrasenia = _hasher.HashPassword(usuario, password);
            await db.SaveChangesAsync();
        }
        return (true, "Acceso correcto.", usuario);
    }

    public async Task<(bool Ok, string Mensaje)> RegistrarAsync(RegistroUsuario model)
    {
        model.Nombre = model.Nombre.Trim();
        model.Email = model.Email.Trim().ToLowerInvariant();
        var errores = new List<ValidationResult>();
        if (!Validator.TryValidateObject(model, new ValidationContext(model), errores, true))
            return (false, string.Join(" ", errores.Select(e => e.ErrorMessage)));
        await using var db = await dbFactory.CreateDbContextAsync();
        if (await db.Usuarios.AnyAsync(u => u.Email == model.Email))
            return (false, "Este email ya está registrado.");
        if (await db.Usuarios.AnyAsync(u => u.Nombre == model.Nombre))
            return (false, "Este nombre de usuario ya está registrado.");

        var usuario = new Usuario { Nombre = model.Nombre, Email = model.Email };
        usuario.Contrasenia = _hasher.HashPassword(usuario, model.Password);
        db.Usuarios.Add(usuario);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "No se pudo registrar el usuario");
            return (false, "No se pudo registrar: el usuario o correo puede estar en uso.");
        }
        logger.LogInformation("Usuario {UsuarioId} registrado", usuario.Id);
        return (true, "Usuario registrado correctamente.");
    }
}
