namespace voltajemoda_backend.DTOs.Inventario;

public class HistorialResponseDto
{
    public int HistorialId { get; set; }
    public string NombreProducto { get; set; } = null!;
    public string SKU_Variante { get; set; } = null!;
    public string NombreEmpleado { get; set; } = null!;
    public int CantidadAjuste { get; set; }
    public string Motivo { get; set; } = null!;
    public DateTime FechaAjuste { get; set; }
}