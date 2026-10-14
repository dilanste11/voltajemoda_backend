using System;
using System.Collections.Generic;

namespace voltajemoda_backend.Models;

public partial class Cupone
{
    public int CuponId { get; set; }

    public string Codigo { get; set; } = null!;

    public decimal Descuento { get; set; }

    public DateTime ValidoDesde { get; set; }

    public DateTime ValidoHasta { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
