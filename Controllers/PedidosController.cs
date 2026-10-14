using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using voltajemoda_backend.DTOs.Transaccional;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Cliente")]
public class PedidosController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public PedidosController(VoltajeDbContext context)
    {
        _context = context;
    }

    private async Task<int?> ObtenerClienteIdAsync()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == ClaimTypes.NameIdentifier);
        if (claim == null) return null;
        int usuarioId = int.Parse(claim.Value);
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
        return cliente?.ClienteId;
    }

    [HttpGet("metodos-pago")]
    public async Task<IActionResult> ObtenerMetodosPago()
    {
        var metodos = await _context.MediosDePagos
            .Where(m => m.Activo == true)
            .Select(m => new { m.MetodoPagoId, m.NombreMetodo })
            .ToListAsync();
        return Ok(metodos);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> ProcesarCheckout([FromBody] RealizarPedidoDto dto)
    {
        var clienteId = await ObtenerClienteIdAsync();
        if (clienteId == null) return Forbid();

        if (!await _context.MediosDePagos.AnyAsync(m => m.MetodoPagoId == dto.MetodoPagoId && m.Activo))
            return BadRequest("El método de pago seleccionado no es válido.");

        var carrito = await _context.Carritos
            .Include(c => c.CarritoItems)
                .ThenInclude(ci => ci.Variante)
                    .ThenInclude(v => v.Producto)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

        if (carrito == null || !carrito.CarritoItems.Any())
            return BadRequest("Tu carrito de compras está vacío.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            decimal subtotalPedido = 0;

            var nuevoPedido = new Pedido
            {
                ClienteId = clienteId.Value,
                DireccionEnvio = dto.DireccionEnvio, 
                CiudadEnvio = dto.CiudadEnvio,       
                MetodoPagoId = dto.MetodoPagoId,
                CuponId = dto.CuponId,
                Estado = "Pendiente",
                FechaPedido = DateTime.Now,
                Subtotal = 0,
                Total = 0
            };

            _context.Pedidos.Add(nuevoPedido);
            await _context.SaveChangesAsync();

            foreach (var item in carrito.CarritoItems)
            {
                var variante = item.Variante;

                if (variante.Stock < item.Cantidad)
                    throw new Exception($"El producto {variante.Producto.Nombre} (SKU: {variante.SkuVariante}) ya no tiene stock suficiente.");

                decimal precioHistorico = variante.Producto.PrecioBase;
                subtotalPedido += (precioHistorico * item.Cantidad);

                var detalle = new DetallesPedido
                {
                    PedidoId = nuevoPedido.PedidoId,
                    VarianteId = item.VarianteId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precioHistorico
                };
                _context.DetallesPedidos.Add(detalle);

                variante.Stock -= item.Cantidad;

                var historial = new HistorialInventario
                {
                    VarianteId = variante.VarianteId,
                    EmpleadoId = null, 
                    CantidadAjuste = -item.Cantidad,
                    Motivo = $"Venta Web - Pedido #{nuevoPedido.PedidoId}",
                    FechaAjuste = DateTime.Now
                };
                _context.HistorialInventarios.Add(historial);
            }

            // 4. Actualizar los totales del pedido y aplicar descuento
            nuevoPedido.Subtotal = subtotalPedido;
            decimal descuento = 0;

            if (dto.CuponId.HasValue)
            {
                // Buscamos el cupón para validar que siga activo justo en el segundo de la compra
                var cupon = await _context.Cupones.FindAsync(dto.CuponId.Value);
                if (cupon != null && cupon.Activo && cupon.ValidoDesde <= DateTime.Now && cupon.ValidoHasta >= DateTime.Now)
                {
                    // Regla de tres simple: (Subtotal * Porcentaje) / 100
                    descuento = subtotalPedido * (cupon.Descuento / 100m); // La 'm' es para forzar el tipo decimal
                }
                else
                {
                    // Si el cupón se venció mientras el usuario tenía el carrito abierto, abortamos la compra
                    throw new Exception("El cupón aplicado ya no es válido o acaba de caducar.");
                }
            }

            nuevoPedido.Total = subtotalPedido - descuento;

            _context.CarritoItems.RemoveRange(carrito.CarritoItems);
            carrito.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                mensaje = "¡Pedido realizado con éxito!",
                pedidoId = nuevoPedido.PedidoId,
                totalPagado = nuevoPedido.Total
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new { error = true, mensaje = ex.Message });
        }
    }

    [HttpGet("mis-pedidos")]
    public async Task<IActionResult> MisPedidos()
    {
        var clienteId = await ObtenerClienteIdAsync();
        if (clienteId == null) return Forbid();

        var pedidos = await _context.Pedidos
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.FechaPedido)
            .Select(p => new
            {
                p.PedidoId,
                p.FechaPedido,
                p.Estado,
                p.Total,
                // Calculamos cuántas prendas compró sumando las cantidades de los detalles
                CantidadArticulos = p.DetallesPedidos.Sum(d => d.Cantidad)
            })
            .ToListAsync();

        return Ok(pedidos);
    }

    [HttpGet("mis-pedidos/{id}")]
    public async Task<IActionResult> MiPedidoDetalle(int id)
    {
        var clienteId = await ObtenerClienteIdAsync();
        if (clienteId == null) return Forbid();

        // El filtro estricto (p.ClienteID == clienteId) evita que un cliente vea la factura de otro
        var pedido = await _context.Pedidos
            .Include(p => p.MetodoPago)
            .Include(p => p.DetallesPedidos)
                .ThenInclude(d => d.Variante)
                    .ThenInclude(v => v.Producto)
            .FirstOrDefaultAsync(p => p.PedidoId == id && p.ClienteId == clienteId);

        if (pedido == null) return NotFound("Pedido no encontrado o no te pertenece.");

        // Construimos una respuesta anónima ágil para el frontend
        var respuesta = new
        {
            pedido.PedidoId,
            pedido.FechaPedido,
            pedido.Estado, // Con esto el frontend sabe si mostrar la barra en "Preparando" o "Enviado"
            pedido.DireccionEnvio,
            pedido.CiudadEnvio,
            MetodoPago = pedido.MetodoPago.NombreMetodo,
            pedido.Subtotal,
            pedido.Total,
            Detalles = pedido.DetallesPedidos.Select(d => new
            {
                d.Variante.Producto.Nombre,
                d.Variante.SkuVariante,
                d.Variante.Talla,
                d.Variante.Color,
                d.Cantidad,
                d.PrecioUnitario,
                Subtotal = d.Cantidad * d.PrecioUnitario
            }).ToList()
        };

        return Ok(respuesta);
    }
}