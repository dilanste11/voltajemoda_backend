namespace voltajemoda_backend.DTOs.Auth;

public class ClienteResponseDto
{
    public int UsuarioId { get; set; } // Necesitamos este ID para poder bloquear o desbloquear al usuario
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public string CorreoElectronico { get; set; } = null!;
    public string? Telefono { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}