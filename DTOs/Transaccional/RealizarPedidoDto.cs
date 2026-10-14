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
    public int MetodoPagoId { get; set; }

    public int? CuponId { get; set; }
}