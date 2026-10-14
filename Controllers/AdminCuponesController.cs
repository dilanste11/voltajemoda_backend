using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voltajemoda_backend.DTOs.Marketing;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador")] // Protección estricta
public class AdminCuponesController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public AdminCuponesController(VoltajeDbContext context)
    {
        _context = context;
    }

    // 1. LISTAR TODOS LOS CUPONES (GET)
    [HttpGet]
    public async Task<IActionResult> ObtenerCupones()
    {
        var cupones = await _context.Cupones
            .OrderByDescending(c => c.ValidoDesde)
            .Select(c => new CuponResponseDto
            {
                CuponId = c.CuponId, 
                Codigo = c.Codigo,
                Descuento = c.Descuento,
                ValidoDesde = c.ValidoDesde,
                ValidoHasta = c.ValidoHasta,
                Activo = c.Activo
            })
            .ToListAsync();

        return Ok(cupones);
    }

    // 2. CREAR NUEVO CUPÓN (POST)
    [HttpPost]
    public async Task<IActionResult> CrearCupon([FromBody] CrearCuponDto dto)
    {
        // Validar que el código no exista previamente
        if (await _context.Cupones.AnyAsync(c => c.Codigo.ToLower() == dto.Codigo.ToLower()))
            return BadRequest($"El código de cupón '{dto.Codigo}' ya existe en el sistema.");

        // Validar lógica de fechas
        if (dto.ValidoHasta <= dto.ValidoDesde)
            return BadRequest("La fecha de finalización debe ser posterior a la fecha de inicio.");

        var nuevoCupon = new Cupone // OJO: Revisa si EF lo llamó Cupone o Cupones (suele quitarle la 's' final)
        {
            Codigo = dto.Codigo.ToUpper(), // Guardar en mayúsculas por convención
            Descuento = dto.Descuento,
            ValidoDesde = dto.ValidoDesde,
            ValidoHasta = dto.ValidoHasta,
            Activo = true
        };

        _context.Cupones.Add(nuevoCupon);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Cupón promocional creado con éxito." });
    }

    // 3. EDITAR CUPÓN EXISTENTE (PUT)
    [HttpPut("{id}")]
    public async Task<IActionResult> EditarCupon(int id, [FromBody] CrearCuponDto dto)
    {
        var cupon = await _context.Cupones.FindAsync(id);
        if (cupon == null) return NotFound("Cupón no encontrado.");

        // Validar que si cambia el código, no choque con otro existente
        if (cupon.Codigo.ToLower() != dto.Codigo.ToLower() &&
            await _context.Cupones.AnyAsync(c => c.Codigo.ToLower() == dto.Codigo.ToLower()))
        {
            return BadRequest($"El código '{dto.Codigo}' ya está siendo usado por otro cupón.");
        }

        if (dto.ValidoHasta <= dto.ValidoDesde)
            return BadRequest("La fecha de finalización debe ser posterior a la fecha de inicio.");

        cupon.Codigo = dto.Codigo.ToUpper();
        cupon.Descuento = dto.Descuento;
        cupon.ValidoDesde = dto.ValidoDesde;
        cupon.ValidoHasta = dto.ValidoHasta;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Cupón actualizado correctamente." });
    }

    // 4. DESACTIVAR CUPÓN (Soft Delete / Toggle - DELETE)
    [HttpDelete("{id}")]
    public async Task<IActionResult> CambiarEstadoCupon(int id)
    {
        var cupon = await _context.Cupones.FindAsync(id);
        if (cupon == null) return NotFound("Cupón no encontrado.");

        cupon.Activo = !cupon.Activo; // Si está activo lo apaga, si está inactivo lo prende
        await _context.SaveChangesAsync();

        string estadoNuevo = cupon.Activo ? "activado" : "desactivado";
        return Ok(new { mensaje = $"El cupón ha sido {estadoNuevo}." });
    }
}