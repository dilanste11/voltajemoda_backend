using System;
using System.Collections.Generic;

namespace voltajemoda_backend.Models;

public partial class HistorialInventario
{
    public int HistorialId { get; set; }

    public int VarianteId { get; set; }

    public int? EmpleadoId { get; set; }

    public int CantidadAjuste { get; set; }

    public string Motivo { get; set; } = null!;

    public DateTime? FechaAjuste { get; set; }

    public virtual Empleado Empleado { get; set; } = null!;

    public virtual Variante Variante { get; set; } = null!;
}
