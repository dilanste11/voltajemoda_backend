using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador, Empleado")]
public class InventarioController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public InventarioController(VoltajeDbContext context)
    {
        _context = context;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> ObtenerResumen()
    {
        var stockBajo = await _context.Variantes
            .Include(v => v.Producto)
            .Where(v => v.Activo && v.Stock <= 5)
            .OrderBy(v => v.Stock)
            .Select(v => new
            {
                v.VarianteId,
                v.ProductoId,
                nombreProducto = v.Producto.Nombre,
                v.SkuVariante,
                v.Talla,
                v.Color,
                v.Stock
            })
            .ToListAsync();

        var resumen = new
        {
            totalVariantes = await _context.Variantes.CountAsync(),
            totalStock = await _context.Variantes.SumAsync(v => (int?)v.Stock) ?? 0,
            variantesSinStock = await _context.Variantes.CountAsync(v => v.Activo && v.Stock == 0),
            stockBajo,
            productosSinStock = await _context.Variantes
                .Include(v => v.Producto)
                .Where(v => v.Activo && v.Stock == 0)
                .Select(v => new
                {
                    v.ProductoId,
                    nombreProducto = v.Producto.Nombre,
                    v.SkuVariante,
                    v.Stock
                })
                .Distinct()
                .ToListAsync()
        };

        return Ok(resumen);
    }
}
