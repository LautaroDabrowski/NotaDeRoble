using InstitutoBlazor.Data;
using InstitutoBlazor.Models;
using Microsoft.EntityFrameworkCore;

namespace InstitutoBlazor.Services;

public class CatalogoService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<List<Departamento>> ObtenerDepartamentosAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Departamentos.OrderBy(d => d.Nombre).AsNoTracking().ToListAsync();
    }

    public async Task<List<Localidad>> ObtenerLocalidadesAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Localidades.OrderBy(l => l.Nombre).AsNoTracking().ToListAsync();
    }
}
