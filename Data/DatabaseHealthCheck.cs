using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace InstitutoBlazor.Data;

/// <summary>Comprueba conexión y esquema usando un contexto de corta duración.</summary>
public class DatabaseHealthCheck(IDbContextFactory<AppDbContext> factory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            await db.Empleados.AsNoTracking().Select(e => e.Id).Take(1).ToListAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Base de datos no disponible.", ex);
        }
    }
}
