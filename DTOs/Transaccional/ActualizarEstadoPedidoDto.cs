using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Transaccional;

public class ActualizarEstadoPedidoDto
{
    [Required(ErrorMessage = "El estado es obligatorio (Ej: Pendiente, Enviado, Entregado, Cancelado).")]
    [StringLength(50)]
    public string Estado { get; set; } = null!;
}