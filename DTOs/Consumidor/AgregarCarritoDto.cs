using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Consumidor;

public class AgregarCarritoDto
{
    [Required(ErrorMessage = "Debes especificar la variante del producto.")]
    public int VarianteId { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "La cantidad debe ser mayor a 0 y razonable para un minorista.")]
    public int Cantidad { get; set; }
}