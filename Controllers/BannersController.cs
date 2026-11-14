using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BannersController : ControllerBase
{
    private readonly VoltajeDbContext _context;
    private readonly ILogger<BannersController> _logger;

    public BannersController(VoltajeDbContext context, ILogger<BannersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerBanners()
    {
        try
        {
            var productos = await _context.Productos
                .AsNoTracking()
                .Include(p => p.ProductoImagenes)
                .Where(p => p.Activo)
                .OrderByDescending(p => p.PrecioBase)
                .Take(5)
                .ToListAsync();

            var destacados = productos.Select(p => new
            {
                id = p.ProductoId,
                titulo = p.Nombre ?? string.Empty,
                descripcion = p.Descripcion,
                precio = p.PrecioBase,
                imagen = p.ProductoImagenes?
                    .OrderBy(i => i.Orden)
                    .Select(i => i.UrlImagen)
                    .FirstOrDefault() ?? string.Empty,
                categoriaId = p.CategoriaId
            })
            .ToList();

            return Ok(destacados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los productos destacados para banners.");
            return StatusCode(500, new { mensaje = "Error al obtener banners", detalle = ex.Message });
        }
    }
}
