using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using voltajemoda_backend.DTOs.Transaccional;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador, Empleado")] // Solo el staff de la tienda
public class AdminPedidosController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public AdminPedidosController(VoltajeDbContext context)
    {
        _context = context;
    }

    // 1. LISTAR TODOS LOS PEDIDOS (Con opción de filtrar por estado)
    [HttpGet]
    public async Task<IActionResult> ObtenerPedidos(
         [FromQuery] string? estado = null,
         [FromQuery] DateTime? fechaInicio = null,
         [FromQuery] DateTime? fechaFin = null,
         [FromQuery] decimal? montoMinimo = null,
         [FromQuery] decimal? montoMaximo = null)
    {
        var query = _context.Pedidos
            .Include(p => p.Cliente)
                .ThenInclude(c => c.Usuario)
            .AsQueryable();

        // Aplicación dinámica de filtros
        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(p => p.Estado.ToLower() == estado.ToLower());

        if (fechaInicio.HasValue)
            query = query.Where(p => p.FechaPedido >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(p => p.FechaPedido <= fechaFin.Value);

        if (montoMinimo.HasValue)
            query = query.Where(p => p.Total >= montoMinimo.Value);

        if (montoMaximo.HasValue)
            query = query.Where(p => p.Total <= montoMaximo.Value);

        var pedidos = await query
            .OrderByDescending(p => p.FechaPedido)
            .Select(p => new
            {
                p.PedidoId,
                Cliente = p.Cliente.Nombre + " " + p.Cliente.Apellido,
                p.CiudadEnvio,
                p.Estado,
                p.Total,
                p.FechaPedido
            })
            .ToListAsync();

        return Ok(pedidos);
    }

    // 2. VER DETALLE EXACTO DE UN PEDIDO (Para armar la caja)
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerDetallePedido(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Cliente).ThenInclude(c => c.Usuario)
            .Include(p => p.MetodoPago)
            .Include(p => p.DetallesPedidos) 
                .ThenInclude(d => d.Variante)
                    .ThenInclude(v => v.Producto)
            .FirstOrDefaultAsync(p => p.PedidoId == id);

        if (pedido == null) return NotFound("Pedido no encontrado.");

        var respuesta = new PedidoAdminResponseDto
        {
            PedidoId = pedido.PedidoId,
            NombreCliente = $"{pedido.Cliente.Nombre} {pedido.Cliente.Apellido}",
            CorreoCliente = pedido.Cliente.Usuario.CorreoElectronico,
            DireccionEnvio = pedido.DireccionEnvio,
            CiudadEnvio = pedido.CiudadEnvio,
            MetodoPago = pedido.MetodoPago.NombreMetodo, // Propiedad NombreMetodo según tu tabla
            Estado = pedido.Estado,
            Total = pedido.Total,
            FechaPedido = pedido.FechaPedido,
            Detalles = pedido.DetallesPedidos.Select(d => new DetallePedidoResponseDto
            {
                NombreProducto = d.Variante.Producto.Nombre,
                SKU_Variante = d.Variante.SkuVariante,
                Talla = d.Variante.Talla,
                Color = d.Variante.Color,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario
            }).ToList()
        };

        return Ok(respuesta);
    }

    // 3. ACTUALIZAR ESTADO DEL PEDIDO (Logística)
    [HttpPut("{id}/estado")]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoPedidoDto dto)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.DetallesPedidos) // Fundamental incluir el detalle
                .ThenInclude(d => d.Variante)
            .FirstOrDefaultAsync(p => p.PedidoId == id);

        if (pedido == null) return NotFound("Pedido no encontrado.");

        if (pedido.Estado == "Entregado" || pedido.Estado == "Cancelado")
            return BadRequest($"El pedido ya está cerrado en estado '{pedido.Estado}'.");

        // Lógica de protección: Si cancelan el pedido, devolvemos la ropa al estante
        if (dto.Estado.Equals("Cancelado", StringComparison.OrdinalIgnoreCase))
        {
            // Intentamos capturar quién es el empleado que canceló
            var claim = User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == ClaimTypes.NameIdentifier);
            int? empleadoId = null;

            if (claim != null)
            {
                int usuarioId = int.Parse(claim.Value);
                var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);
                empleadoId = empleado?.EmpleadoId;
            }

            foreach (var detalle in pedido.DetallesPedidos)
            {
                // Devolvemos el stock a la variante
                detalle.Variante.Stock += detalle.Cantidad;

                // Dejamos la huella en el Kardex
                var historial = new HistorialInventario
                {
                    VarianteId = detalle.VarianteId,
                    EmpleadoId = empleadoId,
                    CantidadAjuste = detalle.Cantidad, // En positivo porque ingresa inventario
                    Motivo = $"Reversión logística - Pedido #{pedido.PedidoId} Cancelado",
                    FechaAjuste = DateTime.Now
                };
                _context.HistorialInventarios.Add(historial);
            }
        }

        pedido.Estado = dto.Estado;
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"El pedido #{id} ha cambiado su estado a: {dto.Estado}" });
    }
}