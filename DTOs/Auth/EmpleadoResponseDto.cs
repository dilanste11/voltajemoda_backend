namespace voltajemoda_backend.DTOs.Auth;

public class EmpleadoResponseDto
{
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public string CorreoElectronico { get; set; } = null!;
    public string Cargo { get; set; } = null!;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}