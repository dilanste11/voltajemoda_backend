using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Transaccional;

public class RealizarPedidoDto
{
    [Required(ErrorMessage = "La dirección de envío es obligatoria.")]
    [StringLength(255)]
    public string DireccionEnvio { get; set; } = null!;

    [Required(ErrorMessage = "La ciudad de envío es obligatoria.")]
    [StringLength(100)]
    public string CiudadEnvio { get; set; } = null!;

    [Required(ErrorMessage = "Debes seleccionar un método de pago.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID debe ser mayor a 0")]
    public int MetodoPagoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El ID debe ser mayor a 0")]
    public int? CuponId { get; set; }
}