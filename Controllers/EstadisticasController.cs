using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador, Empleado")]
public class EstadisticasController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public EstadisticasController(VoltajeDbContext context)
    {
        _context = context;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> ObtenerResumen()
    {
        var hoy = DateTime.Today;
        var primerDiaMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var ventasPorDia = await _context.Pedidos
            .Where(p => p.FechaPedido != null && p.FechaPedido.Value.Date >= hoy.AddDays(-6))
            .GroupBy(p => p.FechaPedido!.Value.Date)
            .Select(g => new
            {
                fecha = g.Key,
                totalVentas = g.Count(),
                ingresos = g.Sum(p => p.Total)
            })
            .OrderBy(x => x.fecha)
            .ToListAsync();

        var resumen = new
        {
            totalVentasMes = await _context.Pedidos
                .Where(p => p.FechaPedido >= primerDiaMes)
                .CountAsync(),
            ingresosMes = await _context.Pedidos
                .Where(p => p.FechaPedido >= primerDiaMes)
                .SumAsync(p => (decimal?)p.Total) ?? 0m,
            promedioPedido = await _context.Pedidos
                .Where(p => p.FechaPedido >= primerDiaMes)
                .AverageAsync(p => (decimal?)p.Total) ?? 0m,
            clientesActivos = await _context.Clientes.CountAsync(),
            productosMasVendidos = await _context.DetallesPedidos
                .Include(d => d.Variante)
                .ThenInclude(v => v.Producto)
                .GroupBy(d => d.Variante.ProductoId)
                .Select(g => new
                {
                    productoId = g.Key,
                    nombre = g.First().Variante.Producto.Nombre,
                    unidadesVendidas = g.Sum(d => d.Cantidad)
                })
                .OrderByDescending(x => x.unidadesVendidas)
                .Take(5)
                .ToListAsync(),
            ventasPorDia
        };

        return Ok(resumen);
    }
}
