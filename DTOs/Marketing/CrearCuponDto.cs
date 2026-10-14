using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Marketing;

public class CrearCuponDto
{
    [Required(ErrorMessage = "El código del cupón es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El código debe tener entre 3 y 50 caracteres.")]
    public string Codigo { get; set; } = null!;

    [Required]
    [Range(0.01, 100.00, ErrorMessage = "El descuento debe ser un porcentaje entre 0.01 y 100.")]
    public decimal Descuento { get; set; }

    [Required]
    public DateTime ValidoDesde { get; set; }

    [Required]
    public DateTime ValidoHasta { get; set; }
}