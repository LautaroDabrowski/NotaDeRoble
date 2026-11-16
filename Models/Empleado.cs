using System.ComponentModel.DataAnnotations;

namespace InstitutoBlazor.Models;

public class Empleado
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [RegularExpression(@"^[0-9]{7,8}$", ErrorMessage = "El DNI debe tener 7 u 8 dígitos, sin puntos.")]
    public string Dni { get; set; } = "";
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    [RegularExpression(@"^[\p{L}][\p{L}\p{M} .'-]*$", ErrorMessage = "El nombre admite letras, espacios, apóstrofes y guiones.")]
    public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres.")]
    [RegularExpression(@"^[\p{L}][\p{L}\p{M} .'-]*$", ErrorMessage = "El apellido admite letras, espacios, apóstrofes y guiones.")]
    public string Apellido { get; set; } = "";
    [RegularExpression(@"^\+?[0-9]{8,15}$", ErrorMessage = "El teléfono debe tener entre 8 y 15 dígitos y puede comenzar con +.")]
    public string? Telefono { get; set; }
    [StringLength(200, ErrorMessage = "La dirección no debe superar los 200 caracteres.")]
    public string? Direccion { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un departamento.")]
    public int IdDepartamento { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una localidad.")]
    public int CodPostal { get; set; }
    public Departamento? Departamento { get; set; }
    public Localidad? Localidad { get; set; }
    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
}
