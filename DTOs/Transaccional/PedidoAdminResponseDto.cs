namespace voltajemoda_backend.DTOs.Transaccional;

public class DetallePedidoResponseDto
{
    public string NombreProducto { get; set; } = null!;
    public string SKU_Variante { get; set; } = null!;
    public string? Talla { get; set; }
    public string? Color { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}

public class PedidoAdminResponseDto
{
    public int PedidoId { get; set; }
    public string NombreCliente { get; set; } = null!;
    public string CorreoCliente { get; set; } = null!;
    public string DireccionEnvio { get; set; } = null!;
    public string CiudadEnvio { get; set; } = null!;
    public string MetodoPago { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public decimal Total { get; set; }
    public DateTime? FechaPedido { get; set; }

    public List<DetallePedidoResponseDto> Detalles { get; set; } = new();
}