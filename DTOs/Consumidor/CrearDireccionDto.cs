using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Consumidor;

public class CrearDireccionDto
{
    [Required(ErrorMessage = "La etiqueta es obligatoria (Ej: Casa, Oficina).")]
    [StringLength(50, ErrorMessage = "La etiqueta no puede exceder los 50 caracteres.")]
    public string Etiqueta { get; set; } = null!;

    [Required(ErrorMessage = "La dirección física es obligatoria.")]
    [StringLength(255, ErrorMessage = "La dirección no puede exceder los 255 caracteres.")]
    public string DireccionFisica { get; set; } = null!;

    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(100, ErrorMessage = "La ciudad no puede exceder los 100 caracteres.")]
    public string Ciudad { get; set; } = null!;
}