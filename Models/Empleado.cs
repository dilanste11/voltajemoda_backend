using System;
using System.Collections.Generic;

namespace voltajemoda_backend.Models;

public partial class Empleado
{
    public int EmpleadoId { get; set; }

    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string? Cargo { get; set; }

    public virtual ICollection<HistorialInventario> HistorialInventarios { get; set; } = new List<HistorialInventario>();

    public virtual Usuario Usuario { get; set; } = null!;
}
