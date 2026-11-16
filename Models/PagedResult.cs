namespace InstitutoBlazor.Models;

/// <summary>Resultado de una consulta paginada; la página empieza en 1.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Pagina, int Tamanio)
{
    public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(Total / (double)Tamanio));
}
