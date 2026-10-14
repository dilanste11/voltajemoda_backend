using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Marketing;

public class ValidarCuponDto
{
    [Required(ErrorMessage = "Debes ingresar un código de cupón.")]
    public string Codigo { get; set; } = null!;
}