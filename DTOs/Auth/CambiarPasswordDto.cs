using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Auth;

public class CambiarPasswordDto
{
    [Required(ErrorMessage = "Debes ingresar tu contraseña actual.")]
    public string PasswordActual { get; set; } = null!;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La nueva contraseña debe tener mínimo 8 caracteres.")]
    public string NuevaPassword { get; set; } = null!;
}