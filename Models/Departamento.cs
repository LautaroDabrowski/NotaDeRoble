namespace InstitutoBlazor.Models;

public class Departamento
{
    public int IdDepartamento { get; set; }
    public string Nombre { get; set; } = "";
    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
