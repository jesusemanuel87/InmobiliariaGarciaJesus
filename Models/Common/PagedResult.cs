namespace InmobiliariaGarciaJesus.Models.Common;

/// <summary>
/// Clase genérica para manejar resultados paginados desde la base de datos.
/// Evita cargar todos los registros en memoria y permite paginación eficiente.
/// </summary>
/// <typeparam name="T">Tipo de entidad a paginar</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Lista de items de la página actual
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Número total de registros que cumplen con los filtros (sin paginación)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Número de página actual (base 1)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Cantidad de items por página
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Número total de páginas calculado
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Indica si existe una página siguiente
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Indica si existe una página anterior
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Número de la primera item en la página actual
    /// </summary>
    public int FirstItemIndex => TotalCount == 0 ? 0 : ((Page - 1) * PageSize) + 1;

    /// <summary>
    /// Número de la última item en la página actual
    /// </summary>
    public int LastItemIndex => Math.Min(Page * PageSize, TotalCount);

    /// <summary>
    /// Constructor vacío
    /// </summary>
    public PagedResult()
    {
    }

    /// <summary>
    /// Constructor con parámetros
    /// </summary>
    public PagedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
