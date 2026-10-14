using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Consumidor;

public class ActualizarCantidadCarritoDto
{
    [Required]
    [Range(1, 100, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }
}