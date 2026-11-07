using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador, Empleado")]
public class DashboardController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public DashboardController(VoltajeDbContext context)
    {
        _context = context;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> ObtenerResumen()
    {
        var hoy = DateTime.Today;
        var primerDiaMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var resumen = new
        {
            totalProductos = await _context.Productos.CountAsync(),
            productosActivos = await _context.Productos.CountAsync(p => p.Activo),
            totalClientes = await _context.Clientes.CountAsync(),
            pedidosPendientes = await _context.Pedidos.CountAsync(p => p.Estado != "Entregado" && p.Estado != "Cancelado"),
            ventasHoy = await _context.Pedidos.CountAsync(p => p.FechaPedido != null && p.FechaPedido.Value.Date == hoy),
            ingresosMes = await _context.Pedidos
                .Where(p => p.FechaPedido >= primerDiaMes)
                .SumAsync(p => (decimal?)p.Total) ?? 0m,
            stockBajo = await _context.Variantes.CountAsync(v => v.Activo && v.Stock <= 5),
            ultimosPedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .ThenInclude(c => c.Usuario)
                .OrderByDescending(p => p.FechaPedido)
                .Take(5)
                .Select(p => new
                {
                    p.PedidoId,
                    cliente = p.Cliente.Nombre + " " + p.Cliente.Apellido,
                    correo = p.Cliente.Usuario.CorreoElectronico,
                    p.Estado,
                    p.Total,
                    p.FechaPedido
                })
                .ToListAsync()
        };

        return Ok(resumen);
    }
}
