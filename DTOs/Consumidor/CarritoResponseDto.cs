namespace voltajemoda_backend.DTOs.Consumidor;

public class CarritoItemResponseDto
{
    public int CarritoItemId { get; set; }
    public int VarianteId { get; set; }
    public string NombreProducto { get; set; } = null!;
    public string SKU_Variante { get; set; } = null!;
    public string? Talla { get; set; }
    public string? Color { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal => PrecioUnitario * Cantidad;
}

public class CarritoResponseDto
{
    public int CarritoId { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public List<CarritoItemResponseDto> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Subtotal);
}