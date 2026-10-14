using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo no es válido.")]
    public string CorreoElectronico { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = null!;
}