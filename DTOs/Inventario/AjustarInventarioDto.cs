using System.ComponentModel.DataAnnotations;

namespace voltajemoda_backend.DTOs.Inventario;

public class AjustarInventarioDto
{
    [Required(ErrorMessage = "Debes ingresar la cantidad (puede ser positiva para ingresos o negativa para salidas).")]
    public int CantidadAjuste { get; set; }

    [Required(ErrorMessage = "El motivo del ajuste es obligatorio (Ej: Ingreso por compra, Producto dañado).")]
    [StringLength(255)]
    public string Motivo { get; set; } = null!;
}