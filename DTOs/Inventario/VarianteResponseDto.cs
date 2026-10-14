namespace voltajemoda_backend.DTOs.Inventario;

public class VarianteResponseDto
{
    public int VarianteId { get; set; }
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = null!;
    public string SKU_Variante { get; set; } = null!;
    public string? Talla { get; set; }
    public string? Color { get; set; }
    public int Stock { get; set; }
    public bool Activo { get; set; }
}