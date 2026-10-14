using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Inventario;

public class AgregarImagenDto
{
    [Required(ErrorMessage = "El ID del producto es obligatorio.")]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "La URL o ruta de la imagen es obligatoria.")]
    [StringLength(255, ErrorMessage = "La URL no puede exceder los 255 caracteres.")]
    public string UrlImagen { get; set; } = null!;

    [Required(ErrorMessage = "El orden de la imagen es obligatorio.")]
    public int Orden { get; set; }
}