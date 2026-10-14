using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Inventario;

public class CrearVarianteDto
{
    [Required(ErrorMessage = "Debes indicar a qué producto pertenece esta variante.")]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "El SKU de la variante es obligatorio.")]
    [StringLength(50, ErrorMessage = "El SKU no puede exceder los 50 caracteres.")]
    public string SKU_Variante { get; set; } = null!;

    [StringLength(20)]
    public string? Talla { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }
}