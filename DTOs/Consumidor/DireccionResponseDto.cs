namespace voltajemoda_backend.DTOs.Consumidor;

public class DireccionResponseDto
{
    public int DireccionId { get; set; }
    public string Etiqueta { get; set; } = null!;
    public string DireccionFisica { get; set; } = null!;
    public string Ciudad { get; set; } = null!;
    public bool Activo { get; set; }
}