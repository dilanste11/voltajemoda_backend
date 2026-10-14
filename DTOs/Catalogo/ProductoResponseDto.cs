namespace voltajemoda_backend.DTOs.Catalogo;

public class ProductoResponseDto
{
    public int ProductoId { get; set; }
    public int CategoriaId { get; set; }
    public string NombreCategoria { get; set; } = null!; // Muy útil para la tienda web
    public string SkuBase { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public decimal PrecioBase { get; set; }
    public bool Activo { get; set; }
}