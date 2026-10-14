using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using voltajemoda_backend.DTOs.Consumidor;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Cliente")]
public class CarritoController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public CarritoController(VoltajeDbContext context)
    {
        _context = context;
    }

    // Método de seguridad para obtener el ClienteID
    private async Task<int?> ObtenerClienteIdAsync()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == ClaimTypes.NameIdentifier);
        if (claim == null) return null;

        int usuarioId = int.Parse(claim.Value);
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
        return cliente?.ClienteId;
    }

    // Método Helper: Obtiene el carrito del cliente o crea uno nuevo si no existe
    private async Task<Carrito> ObtenerOCrearCarritoAsync(int clienteId)
    {
        var carrito = await _context.Carritos.FirstOrDefaultAsync(c => c.ClienteId == clienteId);

        if (carrito == null)
        {
            carrito = new Carrito 
            {
                ClienteId = clienteId,
                FechaActualizacion = DateTime.Now
            };
            _context.Carritos.Add(carrito);
            await _context.SaveChangesAsync();
        }
        return carrito;
    }

    // 1. OBTENER MI CARRITO (GET)
    [HttpGet]
    public async Task<IActionResult> MiCarrito()
    {
        var clienteId = await ObtenerClienteIdAsync();
        if (clienteId == null) return Forbid();

        var carrito = await _context.Carritos
            .Include(c => c.CarritoItems) // Si EF lo nombró de otra forma, ajusta la propiedad
                .ThenInclude(ci => ci.Variante)
                    .ThenInclude(v => v.Producto)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

        if (carrito == null || !carrito.CarritoItems.Any())
            return Ok(new { mensaje = "Tu carrito está vacío.", items = new List<object>(), total = 0 });

        var respuesta = new CarritoResponseDto
        {
            CarritoId = carrito.CarritoId,
            FechaActualizacion = carrito.FechaActualizacion,
            Items = carrito.CarritoItems.Select(ci => new CarritoItemResponseDto
            {
                CarritoItemId = ci.CarritoItemId,
                VarianteId = ci.VarianteId,
                NombreProducto = ci.Variante.Producto.Nombre,
                SKU_Variante = ci.Variante.SkuVariante,
                Talla = ci.Variante.Talla,
                Color = ci.Variante.Color,
                PrecioUnitario = ci.Variante.Producto.PrecioBase, // El precio base viene de Producto
                Cantidad = ci.Cantidad
            }).ToList()
        };

        return Ok(respuesta);
    }

    // 2. AGREGAR ÍTEM AL CARRITO (POST)
    [HttpPost("items")]
    public async Task<IActionResult> AgregarAlCarrito([FromBody] AgregarCarritoDto dto)
    {
        var clienteId = await ObtenerClienteIdAsync();
        if (clienteId == null) return Forbid();

        // Validar que la variante exista y esté activa
        var variante = await _context.Variantes.Include(v => v.Producto).FirstOrDefaultAsync(v => v.VarianteId == dto.VarianteId);
        if (variante == null || !variante.Activo || !variante.Producto.Activo)
            return BadRequest("El producto seleccionado no está disponible.");

        var carrito = await ObtenerOCrearCarritoAsync(clienteId.Value);

        // Buscar si el ítem ya está en el carrito
        var itemExistente = await _context.CarritoItems
            .FirstOrDefaultAsync(ci => ci.CarritoId == carrito.CarritoId && ci.VarianteId == dto.VarianteId);

        int cantidadFinal = itemExistente != null ? itemExistente.Cantidad + dto.Cantidad : dto.Cantidad;

        // Validar Stock
        if (variante.Stock < cantidadFinal)
            return BadRequest($"No hay stock suficiente. Solo quedan {variante.Stock} unidades disponibles.");

        if (itemExistente != null)
        {
            itemExistente.Cantidad = cantidadFinal;
        }
        else
        {
            var nuevoItem = new CarritoItem 
            {
                CarritoItemId = 0,
                CarritoId = carrito.CarritoId,
                VarianteId = dto.VarianteId,
                Cantidad = dto.Cantidad
            };
            _context.CarritoItems.Add(nuevoItem);
        }

        carrito.FechaActualizacion = DateTime.Now;
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Producto agregado al carrito." });
    }

    // 3. ACTUALIZAR CANTIDAD DE UN ÍTEM (PUT)
    [HttpPut("items/{itemId}")]
    public async Task<IActionResult> ActualizarCantidad(int itemId, [FromBody] ActualizarCantidadCarritoDto dto)
    {
        var clienteId = await ObtenerClienteIdAsync();
        if (clienteId == null) return Forbid();

        var item = await _context.CarritoItems
            .Include(ci => ci.Carrito)
            .Include(ci => ci.Variante)
            .FirstOrDefaultAsync(ci => ci.CarritoItemId == itemId && ci.Carrito.ClienteId == clienteId);

        if (item == null) return NotFound("Ítem no encontrado en tu carrito.");

        if (item.Variante.Stock < dto.Cantidad)
            return BadRequest($"Stock insuficiente. Solo quedan {item.Variante.Stock} unidades disponibles.");

        item.Cantidad = dto.Cantidad;
        item.Carrito.FechaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Cantidad actualizada." });
    }

    // 4. ELIMINAR ÍTEM DEL CARRITO (DELETE)
    [HttpDelete("items/{itemId}")]
    public async Task<IActionResult> EliminarItem(int itemId)
    {
        var clienteId = await ObtenerClienteIdAsync();
        if (clienteId == null) return Forbid();

        var item = await _context.CarritoItems
            .Include(ci => ci.Carrito)
            .FirstOrDefaultAsync(ci => ci.CarritoItemId == itemId && ci.Carrito.ClienteId == clienteId);

        if (item == null) return NotFound("Ítem no encontrado.");

        _context.CarritoItems.Remove(item);
        item.Carrito.FechaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Producto removido del carrito." });
    }

    // 5. VACIAR CARRITO COMPLETO (DELETE)
    [HttpDelete]
    public async Task<IActionResult> VaciarCarrito()
    {
        var clienteId = await ObtenerClienteIdAsync();
        if (clienteId == null) return Forbid();

        var carrito = await _context.Carritos
            .Include(c => c.CarritoItems)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

        if (carrito != null && carrito.CarritoItems.Any())
        {
            _context.CarritoItems.RemoveRange(carrito.CarritoItems);
            carrito.FechaActualizacion = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        return Ok(new { mensaje = "Tu carrito ha sido vaciado." });
    }
}