using InstitutoBlazor.Models;
using Microsoft.EntityFrameworkCore;

namespace InstitutoBlazor.Data;

/// <summary>Datos ficticios y deterministas, insertados una sola vez por la migración de seed.</summary>
public static class DemoData
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<Departamento>().HasData(
            new Departamento { IdDepartamento = 1, Nombre = "Ventas" },
            new Departamento { IdDepartamento = 2, Nombre = "Producción" },
            new Departamento { IdDepartamento = 3, Nombre = "Marketing" },
            new Departamento { IdDepartamento = 4, Nombre = "Administración" },
            new Departamento { IdDepartamento = 5, Nombre = "Gerencia" });
        builder.Entity<Localidad>().HasData(
            new Localidad { CodPostal = 1865, Nombre = "San Vicente", Provincia = "Buenos Aires" },
            new Localidad { CodPostal = 1984, Nombre = "Domselaar", Provincia = "Buenos Aires" },
            new Localidad { CodPostal = 1980, Nombre = "Brandsen", Provincia = "Buenos Aires" },
            new Localidad { CodPostal = 1804, Nombre = "Canning", Provincia = "Buenos Aires" },
            new Localidad { CodPostal = 1842, Nombre = "Monte Grande", Provincia = "Buenos Aires" });

        string[] nombres = ["Ana", "Bruno", "Carla", "Diego", "Elena", "Fabio", "Gabriela", "Hugo", "Inés", "Javier", "Lucía", "Martín"];
        string[] apellidos = ["Álvarez", "Benítez", "Castro", "Díaz", "Estévez", "Fernández", "García", "Herrera", "Ibarra", "Juárez", "López", "Molina"];
        int[] localidades = [1865, 1984, 1980, 1804, 1842];
        builder.Entity<Empleado>().HasData(Enumerable.Range(0, nombres.Length).Select(i => new Empleado
        {
            Id = i + 1,
            Dni = (90000001 + i).ToString(System.Globalization.CultureInfo.InvariantCulture),
            Nombre = nombres[i], Apellido = apellidos[i],
            Direccion = $"Calle de demostración {i + 1}",
            IdDepartamento = i % 5 + 1, CodPostal = localidades[i % 5]
        }));
    }
}
