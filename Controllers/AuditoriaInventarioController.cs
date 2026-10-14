using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voltajemoda_backend.DTOs.Inventario;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador")] // Protección estricta: Solo gerencia audita
public class AuditoriaInventarioController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public AuditoriaInventarioController(VoltajeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerHistorialCompleto()
    {
        var historial = await _context.HistorialInventarios
            .Include(h => h.Variante)
                .ThenInclude(v => v.Producto)
            .Include(h => h.Empleado)
            .OrderByDescending(h => h.FechaAjuste) // Los movimientos más recientes primero
            .Select(h => new HistorialResponseDto
            {
                HistorialId = h.HistorialId, // Ajusta a Id si tu modelo difiere
                NombreProducto = h.Variante.Producto.Nombre,
                SKU_Variante = h.Variante.SkuVariante,
                NombreEmpleado = h.Empleado.Nombre + " " + h.Empleado.Apellido,
                CantidadAjuste = h.CantidadAjuste,
                Motivo = h.Motivo,
                FechaAjuste = h.FechaAjuste.GetValueOrDefault()
            })
            .ToListAsync();

        return Ok(historial);
    }
}