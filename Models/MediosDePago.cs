using System;
using System.Collections.Generic;

namespace voltajemoda_backend.Models;

public partial class MediosDePago
{
    public int MetodoPagoId { get; set; }

    public string NombreMetodo { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
