using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using voltajemoda_backend.DTOs.Catalogo;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductosController : ControllerBase
{
    private readonly VoltajeDbContext _context;
    private readonly ILogger<ProductosController> _logger;

    public ProductosController(VoltajeDbContext context, ILogger<ProductosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // 1. LISTAR PRODUCTOS (Público - Para la tienda y el backoffice)
    // 1. CATÁLOGO PÚBLICO (Con filtros y paginación para la tienda web)
    [HttpGet]
    public async Task<IActionResult> ObtenerProductos(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        [FromQuery] int? categoriaId = null,
        [FromQuery] string? busqueda = null,
        [FromQuery] decimal? precioMin = null,
        [FromQuery] decimal? precioMax = null)
    {
        if (pagina < 1 || tamanoPagina < 1)
            return BadRequest(new { mensaje = "La página y el tamaño deben ser mayores que cero." });

        try
        {
            var query = _context.Productos
                .AsNoTracking()
                .Where(p => p.Activo);

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.ToLower();
                query = query.Where(p =>
                    (!string.IsNullOrEmpty(p.Nombre) && p.Nombre.ToLower().Contains(texto))
                    || (!string.IsNullOrEmpty(p.Descripcion) && p.Descripcion.ToLower().Contains(texto)));
            }

            if (precioMin.HasValue)
                query = query.Where(p => p.PrecioBase >= precioMin.Value);

            if (precioMax.HasValue)
                query = query.Where(p => p.PrecioBase <= precioMax.Value);

            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);

            var productos = await query
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .Select(p => new ProductoResponseDto
                {
                    ProductoId = p.ProductoId,
                    CategoriaId = p.CategoriaId,
                    NombreCategoria = p.Categoria == null ? string.Empty : p.Categoria.Nombre,
                    SkuBase = p.SkuBase,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    PrecioBase = p.PrecioBase,
                    Activo = p.Activo
                })
                .ToListAsync();

            var respuesta = new CatalogoPaginadoDto
            {
                Productos = productos,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros
            };

            return Ok(respuesta);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL Server no está disponible al consultar el catálogo.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { mensaje = "Base de datos temporalmente no disponible" });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout al consultar el catálogo.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { mensaje = "Base de datos temporalmente no disponible" });
        }
    }

    // 1.1 OBTENER PRODUCTO POR ID (Público - Detalle con imágenes y variantes)
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerProductoPorId(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.ProductoImagenes)
            .Include(p => p.Variantes)
            .FirstOrDefaultAsync(p => p.ProductoId == id);

        if (producto == null)
            return NotFound(new { error = true, mensaje = "Producto no encontrado." });

        var detalle = new
        {
            producto.ProductoId,
            producto.CategoriaId,
            NombreCategoria = producto.Categoria.Nombre,
            producto.SkuBase,
            producto.Nombre,
            producto.Descripcion,
            producto.PrecioBase,
            producto.Activo,
            Imagenes = producto.ProductoImagenes
                .OrderBy(i => i.Orden)
                .Select(i => new { i.ImagenId, i.UrlImagen, i.Orden }),
            Variantes = producto.Variantes
                .Where(v => v.Activo)
                .Select(v => new
                {
                    v.VarianteId,
                    v.SkuVariante,
                    v.Talla,
                    v.Color,
                    v.Stock,
                    v.Activo
                })
        };

        return Ok(detalle);
    }

    // 2. CREAR PRODUCTO (Protegido)
    [Authorize(Roles = "Administrador, Empleado")]
    [HttpPost]
    public async Task<IActionResult> CrearProducto([FromBody] CrearProductoDto dto)
    {
        // Validar que la categoría que intentan asignar realmente exista
        if (!await _context.Categorias.AnyAsync(c => c.CategoriaId == dto.CategoriaId))
        {
            return BadRequest("La categoría seleccionada no existe en el sistema.");
        }

        // Validar que el SKU sea único (un mismo código de barras no puede estar en dos productos)
        if (await _context.Productos.AnyAsync(p => p.SkuBase.ToLower() == dto.SKU_Base.ToLower()))
        {
            return BadRequest("Ya existe un producto registrado con este SKU Base.");
        }

        var nuevoProducto = new Producto
        {
            CategoriaId = dto.CategoriaId,
            SkuBase = dto.SKU_Base,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            PrecioBase = dto.PrecioBase,
            Activo = true // Nace activo por defecto
        };

        _context.Productos.Add(nuevoProducto);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"El producto '{dto.Nombre}' ha sido creado exitosamente." });
    }

    // 3. EDITAR PRODUCTO (Protegido)
    [Authorize(Roles = "Administrador, Empleado")]
    [HttpPut("{id}")]
    public async Task<IActionResult> EditarProducto(int id, [FromBody] CrearProductoDto dto)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound("Producto no encontrado.");

        if (!await _context.Categorias.AnyAsync(c => c.CategoriaId == dto.CategoriaId))
            return BadRequest("La categoría seleccionada no existe.");

        // Validar que el SKU no choque con el de OTRO producto existente
        if (await _context.Productos.AnyAsync(p => p.SkuBase.ToLower() == dto.SKU_Base.ToLower() && p.ProductoId != id))
            return BadRequest("El SKU ingresado ya está en uso por otro producto.");

        producto.CategoriaId = dto.CategoriaId;
        producto.SkuBase = dto.SKU_Base;
        producto.Nombre = dto.Nombre;
        producto.Descripcion = dto.Descripcion;
        producto.PrecioBase = dto.PrecioBase;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Producto actualizado correctamente." });
    }

    // 4. ACTIVAR / DESACTIVAR PRODUCTO (Protegido)
    [Authorize(Roles = "Administrador, Empleado")]
    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstadoProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound("Producto no encontrado.");

        producto.Activo = !producto.Activo;
        await _context.SaveChangesAsync();

        string estadoActual = producto.Activo ? "Activado" : "Desactivado (Oculto)";
        return Ok(new { mensaje = $"El producto ha sido {estadoActual}." });
    }
}