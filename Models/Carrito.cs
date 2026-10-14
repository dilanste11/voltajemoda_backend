using System;
using System.Collections.Generic;

namespace voltajemoda_backend.Models;

public partial class Carrito
{
    public int CarritoId { get; set; }

    public int ClienteId { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual ICollection<CarritoItem> CarritoItems { get; set; } = new List<CarritoItem>();

    public virtual Cliente Cliente { get; set; } = null!;
}
