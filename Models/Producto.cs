using System;
using System.Collections.Generic;

namespace voltajemoda_backend.Models;

public partial class Producto
{
    public int ProductoId { get; set; }

    public int CategoriaId { get; set; }

    public string SkuBase { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal PrecioBase { get; set; }

    public bool Activo { get; set; }

    public virtual Categoria Categoria { get; set; } = null!;

    public virtual ICollection<ProductoImagene> ProductoImagenes { get; set; } = new List<ProductoImagene>();

    public virtual ICollection<Variante> Variantes { get; set; } = new List<Variante>();
}
