using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Auth;

public class CambiarRolDto
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio (Ej: Administrador, Empleado, Cliente).")]
    public string NuevoRol { get; set; } = null!;

    // Es opcional porque solo se necesita si promovemos un Cliente a Empleado/Admin
    public string? Cargo { get; set; }
}