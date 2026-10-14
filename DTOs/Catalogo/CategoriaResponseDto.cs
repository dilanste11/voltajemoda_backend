namespace voltajemoda_backend.DTOs.Catalogo;

public class CategoriaResponseDto
{
    public int CategoriaId { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; } // Ajustado a "Activo" en lugar de "Activa" según tu tabla
}