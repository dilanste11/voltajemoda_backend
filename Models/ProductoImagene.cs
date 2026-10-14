using System;
using System.Collections.Generic;

namespace voltajemoda_backend.Models;

public partial class ProductoImagene
{
    public int ImagenId { get; set; }

    public int ProductoId { get; set; }

    public string UrlImagen { get; set; } = null!;

    public int Orden { get; set; }

    public virtual Producto Producto { get; set; } = null!;
}
