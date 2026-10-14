using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using voltajemoda_backend.DTOs.Consumidor;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Cliente")] // Estrictamente para compradores
public class DireccionesController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public DireccionesController(VoltajeDbContext context)
    {
        _context = context;
    }

    // Método de seguridad: Convierte el token del usuario en un ClienteID
    private async Task<int?> ObtenerClienteIdDelTokenAsync()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == ClaimTypes.NameIdentifier);
        if (claim == null) return null;

        int usuarioId = int.Parse(claim.Value);
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        return cliente?.ClienteId; // Puede ser null si por alguna razón el perfil de cliente no existe
    }

    // 1. LISTAR MIS DIRECCIONES (GET)
    [HttpGet]
    public async Task<IActionResult> MisDirecciones()
    {
        var clienteId = await ObtenerClienteIdDelTokenAsync();
        if (clienteId == null) return Forbid("No se encontró el perfil de cliente asociado a esta cuenta.");

        var direcciones = await _context.DireccionesEntregas
            .Where(d => d.ClienteId == clienteId && d.Activo == true)
            .Select(d => new DireccionResponseDto
            {
                DireccionId = d.DireccionId, 
                Etiqueta = d.Etiqueta,
                DireccionFisica = d.DireccionFisica,
                Ciudad = d.Ciudad,
                Activo = d.Activo
            })
            .ToListAsync();

        return Ok(direcciones);
    }

    // 2. AGREGAR UNA DIRECCIÓN (POST)
    [HttpPost]
    public async Task<IActionResult> AgregarDireccion([FromBody] CrearDireccionDto dto)
    {
        var clienteId = await ObtenerClienteIdDelTokenAsync();
        if (clienteId == null) return Forbid("No se encontró el perfil de cliente.");

        // Validar límite (Opcional, pero recomendado: no dejar que un cliente guarde 500 direcciones)
        var totalDirecciones = await _context.DireccionesEntregas.CountAsync(d => d.ClienteId == clienteId && d.Activo);
        if (totalDirecciones >= 10) return BadRequest("Has alcanzado el límite máximo de 10 direcciones guardadas.");

       
        var nuevaDireccion = new DireccionesEntrega
        {
            ClienteId = clienteId.Value,
            Etiqueta = dto.Etiqueta,
            DireccionFisica = dto.DireccionFisica,
            Ciudad = dto.Ciudad,
            Activo = true
        };

        _context.DireccionesEntregas.Add(nuevaDireccion);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Dirección guardada exitosamente en tu libreta." });
    }

    // 3. EDITAR UNA DIRECCIÓN (PUT)
    [HttpPut("{id}")]
    public async Task<IActionResult> EditarDireccion(int id, [FromBody] CrearDireccionDto dto)
    {
        var clienteId = await ObtenerClienteIdDelTokenAsync();
        if (clienteId == null) return Forbid();

        // Validamos doble: que el ID de dirección exista Y que pertenezca al cliente logueado
        var direccion = await _context.DireccionesEntregas
            .FirstOrDefaultAsync(d => d.DireccionId == id && d.ClienteId == clienteId);

        if (direccion == null) return NotFound("Dirección no encontrada o no te pertenece.");

        direccion.Etiqueta = dto.Etiqueta;
        direccion.DireccionFisica = dto.DireccionFisica;
        direccion.Ciudad = dto.Ciudad;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Dirección actualizada correctamente." });
    }

    // 4. ELIMINAR (Soft Delete) UNA DIRECCIÓN (DELETE)
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarDireccion(int id)
    {
        var clienteId = await ObtenerClienteIdDelTokenAsync();
        if (clienteId == null) return Forbid();

        var direccion = await _context.DireccionesEntregas
            .FirstOrDefaultAsync(d => d.DireccionId == id && d.ClienteId == clienteId);

        if (direccion == null) return NotFound("Dirección no encontrada o no te pertenece.");

        // Hacemos Soft Delete (La desactivamos en lugar de borrarla de la tabla)
        direccion.Activo = false;
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Dirección eliminada de tu libreta." });
    }
}