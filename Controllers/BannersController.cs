using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BannersController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public BannersController(VoltajeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerBanners()
    {
        var destacados = await _context.Productos
            .Include(p => p.ProductoImagenes)
            .Where(p => p.Activo)
            .OrderByDescending(p => p.PrecioBase)
            .Take(5)
            .Select(p => new
            {
                id = p.ProductoId,
                titulo = p.Nombre,
                descripcion = p.Descripcion,
                precio = p.PrecioBase,
                imagen = p.ProductoImagenes
                    .OrderBy(i => i.Orden)
                    .Select(i => i.UrlImagen)
                    .FirstOrDefault() ?? "",
                categoriaId = p.CategoriaId
            })
            .ToListAsync();

        return Ok(destacados);
    }
}
