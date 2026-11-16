using InstitutoBlazor.Models;
using Microsoft.EntityFrameworkCore;

namespace InstitutoBlazor.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<Localidad> Localidades => Set<Localidad>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.ToTable("empleados");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Dni).HasColumnName("dni").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Apellido).HasColumnName("apellido").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(50);
            entity.Property(e => e.Direccion).HasColumnName("Direccion").HasMaxLength(200);
            entity.Property(e => e.IdDepartamento).HasColumnName("id_Departamento");
            entity.Property(e => e.CodPostal).HasColumnName("CodPostal");
            entity.HasIndex(e => e.Dni).IsUnique();
            entity.HasOne(e => e.Departamento).WithMany(d => d.Empleados)
                .HasForeignKey(e => e.IdDepartamento).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Localidad).WithMany(l => l.Empleados)
                .HasForeignKey(e => e.CodPostal).HasPrincipalKey(l => l.CodPostal)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.ToTable("departamento");
            entity.HasKey(d => d.IdDepartamento);
            entity.Property(d => d.IdDepartamento).HasColumnName("id_Departamento");
            entity.Property(d => d.Nombre).HasColumnName("Nombre").HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Localidad>(entity =>
        {
            entity.ToTable("localidad");
            entity.HasKey(l => l.CodPostal);
            entity.Property(l => l.CodPostal).HasColumnName("CodPostal").ValueGeneratedNever();
            entity.Property(l => l.Nombre).HasColumnName("Nombre").HasMaxLength(100).IsRequired();
            entity.Property(l => l.Provincia).HasColumnName("Provincia").HasMaxLength(100);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuario");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(200).IsRequired();
            entity.Property(u => u.Contrasenia).HasColumnName("contrasenia").HasMaxLength(500).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Nombre).IsUnique();
        });

        DemoData.Configure(modelBuilder);
    }
}
