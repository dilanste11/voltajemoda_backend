using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Auth;

public class RegistroEmpleadoDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    public string Apellido { get; set; } = null!;

    [Required(ErrorMessage = "El cargo es obligatorio (ej. Ventas, Bodega).")]
    public string Cargo { get; set; } = null!;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    public string CorreoElectronico { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mínimo 8 caracteres.")]
    public string Password { get; set; } = null!;
}