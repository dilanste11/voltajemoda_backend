using System;
using System.Collections.Generic;

namespace voltajemoda_backend.Models;

public partial class CarritoItem
{
    public int CarritoItemId { get; set; }

    public int CarritoId { get; set; }

    public int VarianteId { get; set; }

    public int Cantidad { get; set; }

    public virtual Carrito Carrito { get; set; } = null!;

    public virtual Variante Variante { get; set; } = null!;
}
