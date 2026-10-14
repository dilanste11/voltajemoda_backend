using System;
using System.Collections.Generic;

namespace voltajemoda_backend.Models;

public partial class DireccionesEntrega
{
    public int DireccionId { get; set; }

    public int ClienteId { get; set; }

    public string Etiqueta { get; set; } = null!;

    public string DireccionFisica { get; set; } = null!;

    public string Ciudad { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
