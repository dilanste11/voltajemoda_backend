namespace voltajemoda_backend.DTOs.Catalogo;

public class ProductoResponseDto
{
    public int ProductoId { get; set; }
    public int CategoriaId { get; set; }
    public string NombreCategoria { get; set; } = null!; // Muy útil para la tienda web
    public string SkuBase { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public decimal PrecioBase { get; set; }
    public bool Activo { get; set; }
    public int CantidadVariantes { get; set; }
    public int Stock { get; set; }

    // Datos livianos para que las tarjetas del catálogo muestren imagen, tallas/colores y stock
    // sin tener que pedir el detalle de cada producto por separado.
    public List<ProductoImagenResumenDto> Imagenes { get; set; } = new();
    public List<VarianteResumenDto> Variantes { get; set; } = new();
}

public class ProductoImagenResumenDto
{
    public int ImagenId { get; set; }
    public string UrlImagen { get; set; } = null!;
    public int Orden { get; set; }
}

public class VarianteResumenDto
{
    public int VarianteId { get; set; }
    public string SkuVariante { get; set; } = null!;
    public string? Talla { get; set; }
    public string? Color { get; set; }
    public int Stock { get; set; }
    public bool Activo { get; set; }
}
