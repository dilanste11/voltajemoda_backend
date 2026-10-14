using System;
using System.Collections.Generic;

namespace voltajemoda_backend.Models;

public partial class Cliente
{
    public int ClienteId { get; set; }

    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string? Telefono { get; set; }

    public virtual Carrito? Carrito { get; set; }

    public virtual ICollection<DireccionesEntrega> DireccionesEntregas { get; set; } = new List<DireccionesEntrega>();

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public virtual Usuario Usuario { get; set; } = null!;
}
