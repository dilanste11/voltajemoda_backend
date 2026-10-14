using System;
using System.Collections.Generic;

namespace voltajemoda_backend.Models;

public partial class DetallesPedido
{
    public int DetalleId { get; set; }

    public int PedidoId { get; set; }

    public int VarianteId { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public virtual Pedido Pedido { get; set; } = null!;

    public virtual Variante Variante { get; set; } = null!;
}
