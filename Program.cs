using InstitutoBlazor.Components;
using InstitutoBlazor.Data;
using InstitutoBlazor.Services;
using InstitutoBlazor.Models;
using InstitutoBlazor.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<IValidator<Empleado>, EmpleadoValidator>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<CatalogoService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<EmpleadosState>();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("sqlserver");

// Nginx corre en la misma VM; no se aceptan encabezados de proxies arbitrarios.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(System.Net.IPAddress.Loopback);
});
var dataProtection = builder.Services.AddDataProtection().SetApplicationName("InstitutoBlazor");
if (builder.Configuration["DataProtection:KeysPath"] is { Length: > 0 } keysPath)
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));

// ProtectedSessionStorage hace persistente la sesión entre recargas (F5).
builder.Services.TryAddScoped<ProtectedSessionStorage>();

var app = builder.Build();

// Activar solo sobre una base nueva o que ya tenga historial de EF; ver docs/BASE-DE-DATOS.md.
if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await using var db = await app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
    await db.Database.MigrateAsync();
}

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();
app.MapHealthChecks("/health");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
