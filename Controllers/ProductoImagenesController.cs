using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voltajemoda_backend.DTOs.Inventario;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductoImagenesController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public ProductoImagenesController(VoltajeDbContext context)
    {
        _context = context;
    }

    // 1. OBTENER IMÁGENES DE UN PRODUCTO (Público)
    [HttpGet("producto/{productoId}")]
    public async Task<IActionResult> ObtenerImagenes(int productoId)
    {
        var imagenes = await _context.ProductoImagenes
            .Where(i => i.ProductoId == productoId)
            .OrderBy(i => i.Orden) // Ahora ordenamos por tu campo Orden
            .Select(i => new
            {
                i.ImagenId, // Coincide con tu PK
                i.ProductoId, // Coincide con tu FK
                i.UrlImagen,
                i.Orden // Coincide con tu campo entero
            })
            .ToListAsync();

        return Ok(imagenes);
    }

    // 2. AGREGAR IMAGEN (Protegido)
    [Authorize(Roles = "Administrador, Empleado")]
    [HttpPost]
    public async Task<IActionResult> AgregarImagen([FromBody] AgregarImagenDto dto)
    {
        if (!await _context.Productos.AnyAsync(p => p.ProductoId == dto.ProductoId))
            return BadRequest("El producto especificado no existe.");

        var nuevaImagen = new ProductoImagene
        {
            ProductoId = dto.ProductoId,
            UrlImagen = dto.UrlImagen,
            Orden = dto.Orden // Guardamos el orden numérico
        };

        _context.ProductoImagenes.Add(nuevaImagen);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Imagen agregada correctamente al producto." });
    }

    // 3. ELIMINAR IMAGEN (Protegido)
    [Authorize(Roles = "Administrador, Empleado")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarImagen(int id)
    {
        var imagen = await _context.ProductoImagenes.FindAsync(id);
        if (imagen == null) return NotFound("Imagen no encontrada.");

        _context.ProductoImagenes.Remove(imagen);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Imagen eliminada exitosamente." });
    }
}