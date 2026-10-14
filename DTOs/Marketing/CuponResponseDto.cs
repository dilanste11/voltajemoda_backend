namespace voltajemoda_backend.DTOs.Marketing;

public class CuponResponseDto
{
    public int CuponId { get; set; }
    public string Codigo { get; set; } = null!;
    public decimal Descuento { get; set; }
    public DateTime ValidoDesde { get; set; }
    public DateTime ValidoHasta { get; set; }
    public bool Activo { get; set; }

    // Propiedad calculada útil para el panel administrativo
    public string EstadoActual
    {
        get
        {
            if (!Activo) return "Inactivo";
            var hoy = DateTime.Now;
            if (hoy < ValidoDesde) return "Programado";
            if (hoy > ValidoHasta) return "Vencido";
            return "Vigente";
        }
    }
}