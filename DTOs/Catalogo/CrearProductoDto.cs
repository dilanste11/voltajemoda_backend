using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Catalogo;

public class CrearProductoDto
{
    [Required(ErrorMessage = "Debes seleccionar una categoría.")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "El SKU Base es obligatorio.")]
    [StringLength(50, ErrorMessage = "El SKU no puede exceder los 50 caracteres.")]
    public string SKU_Base { get; set; } = null!;

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres.")]
    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El precio base es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal PrecioBase { get; set; }
}