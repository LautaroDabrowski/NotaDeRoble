using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InstitutoBlazor.Data;

/// <summary>Permite generar migraciones sin iniciar Blazor ni conectarse a una base.</summary>
public class AppDbContextDesignFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<AppDbContextDesignFactory>()
            .AddEnvironmentVariables().Build();
        var connection = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connection))
            connection = "Server=localhost;Database=InstitutoBlazorDemo;Integrated Security=True;TrustServerCertificate=True";
        return new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connection).Options);
    }
}
