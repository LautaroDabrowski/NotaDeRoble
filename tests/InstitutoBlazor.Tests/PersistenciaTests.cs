using InstitutoBlazor.Data;
using InstitutoBlazor.Models;
using InstitutoBlazor.Services;
using InstitutoBlazor.Validators;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InstitutoBlazor.Tests;

// Por defecto usa SQLite relacional solo en tests. CI puede ejecutar los mismos casos contra SQL Server.
public sealed class PersistenciaTests : IAsyncLifetime, IDbContextFactory<AppDbContext>
{
    private SqliteConnection? _sqlite;
    private DbContextOptions<AppDbContext> _options = default!;
    private EmpleadoService _service = default!;
    private bool _sqlServer;

    public AppDbContext CreateDbContext() => new(_options);

    public async Task InitializeAsync()
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        var connection = Environment.GetEnvironmentVariable("INSTITUTO_TEST_SQLSERVER");
        _sqlServer = !string.IsNullOrWhiteSpace(connection);
        if (_sqlServer)
        {
            var sql = new SqlConnectionStringBuilder(connection)
            {
                InitialCatalog = $"InstitutoBlazorTest_{Guid.NewGuid():N}"
            };
            builder.UseSqlServer(sql.ConnectionString);
        }
        else
        {
            _sqlite = new SqliteConnection("Data Source=:memory:");
            await _sqlite.OpenAsync();
            builder.UseSqlite(_sqlite);
        }
        _options = builder.Options;
        await using var db = CreateDbContext();
        if (_sqlServer) await db.Database.MigrateAsync();
        else await db.Database.EnsureCreatedAsync();
        _service = new(this, new EmpleadoValidator(this), NullLogger<EmpleadoService>.Instance);
    }

    public async Task DisposeAsync()
    {
        if (_sqlServer)
        {
            await using var db = CreateDbContext();
            await db.Database.EnsureDeletedAsync(); // Nombre aleatorio creado exclusivamente por este test.
        }
        if (_sqlite is not null) await _sqlite.DisposeAsync();
    }

    private static Empleado Nuevo(string dni = "12345678") => new()
    {
        Dni = dni, Nombre = "José María", Apellido = "O'Brien", Telefono = "+541134567890",
        Direccion = "Calle de prueba 123", IdDepartamento = 1, CodPostal = 1865
    };

    [Fact]
    public async Task Crud_PersisteEnContextosIndependientes_ConRelaciones()
    {
        var empleado = Nuevo();
        Assert.True((await _service.CrearAsync(empleado)).Ok);
        Assert.True(empleado.Id > 12);
        var guardado = await _service.ObtenerPorIdAsync(empleado.Id);
        Assert.Equal("Ventas", guardado!.Departamento!.Nombre);
        Assert.Equal("San Vicente", guardado.Localidad!.Nombre);
        guardado.Apellido = "Pérez";
        Assert.True((await _service.ActualizarAsync(guardado)).Ok);
        Assert.Equal("Pérez", (await _service.ObtenerPorIdAsync(empleado.Id))!.Apellido);
        Assert.True((await _service.EliminarAsync(empleado.Id)).Ok);
        Assert.Null(await _service.ObtenerPorIdAsync(empleado.Id));
        Assert.False((await _service.EliminarAsync(empleado.Id)).Ok);
    }

    [Fact]
    public async Task DniDuplicado_NoSeGuarda_YEditarPropioDniEsValido()
    {
        var primero = Nuevo();
        Assert.True((await _service.CrearAsync(primero)).Ok);
        Assert.False((await _service.CrearAsync(Nuevo())).Ok);
        Assert.True((await _service.ActualizarAsync(primero)).Ok);
        var otro = Nuevo("23456789");
        await _service.CrearAsync(otro);
        otro.Dni = primero.Dni;
        Assert.False((await _service.ActualizarAsync(otro)).Ok);
        Assert.Equal("23456789", (await _service.ObtenerPorIdAsync(otro.Id))!.Dni);
    }

    [Fact]
    public async Task IndiceUnico_ProtegeInclusoSiSeOmiteElValidador()
    {
        await using var db = CreateDbContext();
        db.Empleados.Add(Nuevo("90000001"));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Paginacion_BusquedaYDepartamentoSeCombinan_AjustaUltimaPagina()
    {
        var primera = await _service.ObtenerPaginadoAsync();
        var segunda = await _service.ObtenerPaginadoAsync(pagina: 2);
        Assert.Equal(12, primera.Total);
        Assert.Equal(10, primera.Items.Count);
        Assert.Equal(2, segunda.Items.Count);
        Assert.Empty(primera.Items.Select(e => e.Id).Intersect(segunda.Items.Select(e => e.Id)));
        Assert.Equal(3, (await _service.ObtenerPaginadoAsync("900000", departamento: 1)).Total);
        Assert.Single((await _service.ObtenerPaginadoAsync("Bruno", departamento: 2)).Items);
        Assert.Empty((await _service.ObtenerPaginadoAsync("Bruno", departamento: 1)).Items);
        foreach (var empleado in segunda.Items) await _service.EliminarAsync(empleado.Id);
        var ultima = await _service.ObtenerPaginadoAsync(pagina: 2);
        Assert.Equal(1, ultima.Pagina);
        Assert.Equal(10, ultima.Items.Count);
    }

    [Fact]
    public async Task Validaciones_RechazanDatosInvalidosYRelacionesInexistentes()
    {
        var empleado = Nuevo("12.345.678");
        Assert.False((await _service.CrearAsync(empleado)).Ok);
        empleado = Nuevo(); empleado.IdDepartamento = 999;
        Assert.False((await _service.CrearAsync(empleado)).Ok);
        empleado = Nuevo(); empleado.CodPostal = 9999;
        Assert.False((await _service.CrearAsync(empleado)).Ok);
        empleado = Nuevo(); empleado.Telefono = "abc";
        Assert.False((await _service.CrearAsync(empleado)).Ok);
        Assert.Equal(12, (await _service.ObtenerEstadisticasAsync()).Empleados);
    }

    [Fact]
    public async Task Registro_ConfirmaPassword_GuardaHashYPermiteLogin()
    {
        var service = new UsuarioService(this, NullLogger<UsuarioService>.Instance);
        var password = $"Prueba{Guid.NewGuid():N}";
        var model = new RegistroUsuario { Nombre = "tester", Email = "TEST@example.test", Password = password, ConfirmarPassword = "distinta" };
        Assert.False((await service.RegistrarAsync(model)).Ok);
        model.ConfirmarPassword = password;
        Assert.True((await service.RegistrarAsync(model)).Ok);
        Assert.False((await service.RegistrarAsync(model)).Ok);
        var login = await service.ValidarAsync("test@example.test", password);
        Assert.True(login.Ok);
        Assert.NotEqual(password, login.Usuario!.Contrasenia);
        Assert.False((await service.ValidarAsync("tester", "incorrecta")).Ok);
    }

    [Fact]
    public async Task Seed_NoResucitaEmpleadosEliminados_AlReiniciarMigraciones()
    {
        await _service.EliminarAsync(1);
        await using var db = CreateDbContext();
        if (_sqlServer) await db.Database.MigrateAsync();
        else await db.Database.EnsureCreatedAsync();
        Assert.Equal(11, await db.Empleados.CountAsync());
        Assert.Equal(5, await db.Departamentos.CountAsync());
        Assert.Equal(5, await db.Localidades.CountAsync());
    }
}
