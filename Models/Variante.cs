using System;
using System.Collections.Generic;

namespace voltajemoda_backend.Models;

public partial class Variante
{
    public int VarianteId { get; set; }

    public int ProductoId { get; set; }

    public string SkuVariante { get; set; } = null!;

    public string? Talla { get; set; }

    public string? Color { get; set; }

    public int Stock { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<CarritoItem> CarritoItems { get; set; } = new List<CarritoItem>();

    public virtual ICollection<DetallesPedido> DetallesPedidos { get; set; } = new List<DetallesPedido>();

    public virtual ICollection<HistorialInventario> HistorialInventarios { get; set; } = new List<HistorialInventario>();

    public virtual Producto Producto { get; set; } = null!;
}
