using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using voltajemoda_backend.DTOs.Auth;
using voltajemoda_backend.Models;
using voltajemoda_backend.Services;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Exige estar logueado, sin importar el rol
public class PerfilController : ControllerBase
{
    private readonly VoltajeDbContext _context;
    private readonly IPasswordHasherService _passwordHasher;

    public PerfilController(VoltajeDbContext context, IPasswordHasherService passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // -----------------------------------------------------------
    // MÉTODO PRIVADO: Extrae el ID real y seguro desde el Token JWT
    // -----------------------------------------------------------
    private bool TryObtenerUsuarioIdDelToken(out int usuarioId)
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == ClaimTypes.NameIdentifier);
        return int.TryParse(claim?.Value, out usuarioId);
    }

    // -----------------------------------------------------------
    // 1. OBTENER MI PERFIL (GET /api/Perfil)
    // -----------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> MiPerfil()
    {
        if (!TryObtenerUsuarioIdDelToken(out var usuarioId))
            return Unauthorized();

        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

        if (usuario == null) return NotFound("Usuario no encontrado.");
        var nombreRol = usuario.Rol?.NombreRol;
        if (string.IsNullOrWhiteSpace(nombreRol))
            return BadRequest("El usuario no tiene un rol válido.");

        // Dependiendo del rol, buscamos en la tabla correspondiente
        if (nombreRol == "Cliente")
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            if (cliente == null) return NotFound("Perfil de cliente no encontrado.");
            return Ok(new { usuario.CorreoElectronico, Rol = nombreRol, cliente.Nombre, cliente.Apellido, cliente.Telefono });
        }
        else
        {
            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);
            if (empleado == null) return NotFound("Perfil de empleado no encontrado.");
            return Ok(new { usuario.CorreoElectronico, Rol = nombreRol, empleado.Nombre, empleado.Apellido, empleado.Cargo });
        }
    }

    // -----------------------------------------------------------
    // 2. ACTUALIZAR MIS DATOS (PUT /api/Perfil)
    // -----------------------------------------------------------
    [HttpPut]
    public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilDto dto)
    {
        if (!TryObtenerUsuarioIdDelToken(out var usuarioId))
            return Unauthorized();
        var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
        if (usuario == null) return NotFound("Usuario no encontrado.");
        var nombreRol = usuario.Rol?.NombreRol;
        if (string.IsNullOrWhiteSpace(nombreRol))
            return BadRequest("El usuario no tiene un rol válido.");

        if (nombreRol == "Cliente")
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            if (cliente == null) return NotFound("Perfil de cliente no encontrado.");
            cliente.Nombre = dto.Nombre;
            cliente.Apellido = dto.Apellido;
            cliente.Telefono = dto.Telefono;
        }
        else
        {
            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);
            if (empleado == null) return NotFound("Perfil de empleado no encontrado.");
            empleado.Nombre = dto.Nombre;
            empleado.Apellido = dto.Apellido;
            // No permitimos que el empleado se cambie el Cargo a sí mismo
        }

        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Perfil actualizado correctamente." });
    }

    // -----------------------------------------------------------
    // 3. CAMBIAR MI CONTRASEÑA (PUT /api/Perfil/password)
    // -----------------------------------------------------------
    [HttpPut("password")]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
    {
        if (!TryObtenerUsuarioIdDelToken(out var usuarioId))
            return Unauthorized();
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null) return NotFound("Usuario no encontrado.");

        // Validar que la contraseña actual sea correcta
        if (!_passwordHasher.VerifyPassword(dto.PasswordActual, usuario.PasswordHash))
        {
            return BadRequest("La contraseña actual es incorrecta.");
        }

        // Hashear y guardar la nueva
        usuario.PasswordHash = _passwordHasher.HashPassword(dto.NuevaPassword);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Contraseña actualizada de forma segura." });
    }
}