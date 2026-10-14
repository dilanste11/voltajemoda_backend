using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voltajemoda_backend.DTOs.Catalogo;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriasController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public CategoriasController(VoltajeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerCategorias()
    {
        var categorias = await _context.Categorias
            .Select(c => new CategoriaResponseDto
            {
                CategoriaId = c.CategoriaId,
                Nombre = c.Nombre,
                Activo = c.Activo // Ajustado según tu tabla
            })
            .ToListAsync();

        return Ok(categorias);
    }

    [Authorize(Roles = "Administrador, Empleado")]
    [HttpPost]
    public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaDto dto)
    {
        if (await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.ToLower()))
        {
            return BadRequest("Ya existe una categoría con este nombre.");
        }

        var nuevaCategoria = new Categoria
        {
            Nombre = dto.Nombre,
            Activo = true
        };

        _context.Categorias.Add(nuevaCategoria);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Categoría '{dto.Nombre}' creada exitosamente." });
    }

    [Authorize(Roles = "Administrador, Empleado")]
    [HttpPut("{id}")]
    public async Task<IActionResult> EditarCategoria(int id, [FromBody] CrearCategoriaDto dto)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound("Categoría no encontrada.");

        if (await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.ToLower() && c.CategoriaId != id))
        {
            return BadRequest("El nombre ingresado ya está en uso por otra categoría.");
        }

        categoria.Nombre = dto.Nombre;
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Categoría actualizada correctamente." });
    }

    [Authorize(Roles = "Administrador, Empleado")]
    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstadoCategoria(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound("Categoría no encontrada.");

        categoria.Activo = !categoria.Activo;
        await _context.SaveChangesAsync();

        string estadoActual = categoria.Activo ? "Activada" : "Desactivada (Oculta)";
        return Ok(new { mensaje = $"La categoría ha sido {estadoActual}." });
    }
}