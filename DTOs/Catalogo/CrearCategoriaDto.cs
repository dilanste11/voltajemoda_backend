using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Catalogo;

public class CrearCategoriaDto
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = null!;
}