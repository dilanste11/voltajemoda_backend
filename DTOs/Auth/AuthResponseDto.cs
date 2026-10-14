namespace voltajemoda_backend.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string Rol { get; set; } = null!;
    public DateTime Expiracion { get; set; }
}