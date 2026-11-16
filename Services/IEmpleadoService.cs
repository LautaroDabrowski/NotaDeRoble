using InstitutoBlazor.Models;

namespace InstitutoBlazor.Services;

/// <summary>Acceso a empleados con un contexto EF Core independiente por operación.</summary>
public interface IEmpleadoService
{
    /// <summary>Busca en DNI, nombre y apellido y pagina en SQL con un orden estable.</summary>
    /// <param name="texto">Texto de búsqueda; vacío devuelve todos los resultados.</param>
    /// <param name="departamento">Id del departamento; cero no filtra.</param>
    /// <param name="pagina">Página solicitada, desde 1; se ajusta al rango disponible.</param>
    /// <param name="tamanio">Registros por página, limitado entre 1 y 50.</param>
    /// <returns>Elementos con sus relaciones, total y página efectiva.</returns>
    Task<PagedResult<Empleado>> ObtenerPaginadoAsync(string? texto = null, int departamento = 0, int pagina = 1, int tamanio = 10);

    /// <summary>Obtiene el detalle y las relaciones de un empleado.</summary>
    /// <param name="id">Identificador del empleado.</param>
    /// <returns>Empleado encontrado o null si fue eliminado o no existe.</returns>
    Task<Empleado?> ObtenerPorIdAsync(int id);

    /// <summary>Valida y crea un empleado; actualiza su Id después de guardar.</summary>
    /// <param name="empleado">Datos del formulario.</param>
    /// <returns>Éxito y mensaje apto para mostrar al usuario.</returns>
    Task<(bool Ok, string Mensaje)> CrearAsync(Empleado empleado);

    /// <summary>Valida y actualiza únicamente los campos editables del empleado.</summary>
    /// <param name="empleado">Datos e Id que se desea modificar.</param>
    /// <returns>Éxito y mensaje, incluyendo duplicados y registros inexistentes.</returns>
    Task<(bool Ok, string Mensaje)> ActualizarAsync(Empleado empleado);

    /// <summary>Elimina un empleado que fue confirmado en la interfaz.</summary>
    /// <param name="id">Identificador del registro.</param>
    /// <returns>Éxito y mensaje del resultado.</returns>
    Task<(bool Ok, string Mensaje)> EliminarAsync(int id);

    /// <summary>Cuenta empleados y departamentos con empleados en la base.</summary>
    /// <returns>Conteos globales, independientes de los filtros de la lista.</returns>
    Task<(int Empleados, int Departamentos)> ObtenerEstadisticasAsync();
}
