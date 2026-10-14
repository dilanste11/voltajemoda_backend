namespace voltajemoda_backend.DTOs.Catalogo;

public class CatalogoPaginadoDto
{
    public List<ProductoResponseDto> Productos { get; set; } = new();
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalRegistros { get; set; }
}