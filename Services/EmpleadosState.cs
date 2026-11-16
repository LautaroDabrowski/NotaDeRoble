namespace InstitutoBlazor.Services;

/// <summary>Filtros y estadísticas del circuito actual; notifica a los componentes suscriptos.</summary>
public class EmpleadosState
{
    public string Busqueda { get; set; } = "";
    public int Departamento { get; set; }
    public int Pagina { get; set; } = 1;
    public int? Total { get; private set; }
    public int Departamentos { get; private set; }
    public event Action? OnChange;

    public void ActualizarResumen(int total, int departamentos)
    {
        Total = total;
        Departamentos = departamentos;
        OnChange?.Invoke();
    }

    public void Limpiar()
    {
        Busqueda = "";
        Departamento = 0;
        Pagina = 1;
        Total = null;
        Departamentos = 0;
        OnChange?.Invoke();
    }
}
