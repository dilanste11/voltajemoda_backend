using System;
using System.Collections.Generic;

namespace voltajemoda_backend.Models;

public partial class Pedido
{
    public int PedidoId { get; set; }

    public int ClienteId { get; set; }

    public string DireccionEnvio { get; set; } = null!;

    public string CiudadEnvio { get; set; } = null!;

    public int MetodoPagoId { get; set; }

    public int? CuponId { get; set; }

    public string Estado { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public decimal Total { get; set; }

    public DateTime? FechaPedido { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Cupone? Cupon { get; set; }

    public virtual ICollection<DetallesPedido> DetallesPedidos { get; set; } = new List<DetallesPedido>();

    public virtual DireccionesEntrega Direccion { get; set; } = null!;

    public virtual MediosDePago MetodoPago { get; set; } = null!;
}
