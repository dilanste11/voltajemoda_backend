using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using voltajemoda_backend.DTOs.Inventario;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VariantesController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public VariantesController(VoltajeDbContext context)
    {
        _context = context;
    }

    // Método privado para extraer el ID del token (igual que en PerfilController)
    private int ObtenerUsuarioIdDelToken()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }

    // 1. LISTAR VARIANTES DE UN PRODUCTO (Público - Para que la tienda muestre tallas y colores)
    [HttpGet("producto/{productoId}")]
    public async Task<IActionResult> ObtenerVariantesPorProducto(int productoId)
    {
        var variantes = await _context.Variantes
            .Include(v => v.Producto)
            .Where(v => v.ProductoId == productoId)
            .Select(v => new VarianteResponseDto
            {
                VarianteId = v.VarianteId, // Ajusta si en tu modelo C# se llama VarianteId
                ProductoId = v.ProductoId,
                NombreProducto = v.Producto.Nombre,
                SKU_Variante = v.SkuVariante,
                Talla = v.Talla,
                Color = v.Color,
                Stock = v.Stock,
                Activo = v.Activo
            })
            .ToListAsync();

        return Ok(variantes);
    }

    // 2. CREAR NUEVA VARIANTE (Protegido)
    [Authorize(Roles = "Administrador, Empleado")]
    [HttpPost]
    public async Task<IActionResult> CrearVariante([FromBody] CrearVarianteDto dto)
    {
        if (!await _context.Productos.AnyAsync(p => p.ProductoId == dto.ProductoId))
            return BadRequest("El producto especificado no existe.");

        if (await _context.Variantes.AnyAsync(v => v.SkuVariante.ToLower() == dto.SKU_Variante.ToLower()))
            return BadRequest("Ya existe una variante con este SKU exacto.");

        var nuevaVariante = new Variante
        {
            ProductoId = dto.ProductoId,
            SkuVariante = dto.SKU_Variante,
            Talla = dto.Talla,
            Color = dto.Color,
            Stock = 0, // Nace en cero como indica tu Base de Datos
            Activo = true
        };

        _context.Variantes.Add(nuevaVariante);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Variante creada exitosamente. Recuerda realizar un ajuste de inventario para sumarle stock." });
    }

    // 3. AJUSTAR INVENTARIO (Protegido - El corazón del Kardex)
    [Authorize(Roles = "Administrador, Empleado")]
    [HttpPost("{id}/inventario")]
    public async Task<IActionResult> AjustarInventario(int id, [FromBody] AjustarInventarioDto dto)
    {
        // 1. Buscar la variante
        var variante = await _context.Variantes.FindAsync(id);
        if (variante == null) return NotFound("Variante no encontrada.");

        // 2. Validar que la salida no deje el stock en negativo
        if (variante.Stock + dto.CantidadAjuste < 0)
            return BadRequest($"Operación inválida. No hay suficiente stock. Stock actual: {variante.Stock}");

        // 3. Obtener al empleado responsable de este movimiento
        int usuarioId = ObtenerUsuarioIdDelToken();
        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);

        if (empleado == null)
            return BadRequest("Tu cuenta no está vinculada a un perfil de empleado operativo válido para mover mercancía.");

        // 4. Modificar el stock
        variante.Stock += dto.CantidadAjuste;

        // 5. Dejar la firma en el historial
        var historial = new HistorialInventario
        {
            VarianteId = id,
            EmpleadoId = empleado.EmpleadoId,
            CantidadAjuste = dto.CantidadAjuste,
            Motivo = dto.Motivo,
            FechaAjuste = DateTime.Now
        };

        _context.HistorialInventarios.Add(historial);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Inventario actualizado. El nuevo stock disponible es: {variante.Stock}." });
    }
}