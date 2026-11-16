namespace InstitutoBlazor.Models;

public class Localidad
{
    public int CodPostal { get; set; }
    public string Nombre { get; set; } = "";
    public string Provincia { get; set; } = "Buenos Aires";
    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
