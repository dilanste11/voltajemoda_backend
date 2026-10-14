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
    private int ObtenerUsuarioIdDelToken()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }

    // -----------------------------------------------------------
    // 1. OBTENER MI PERFIL (GET /api/Perfil)
    // -----------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> MiPerfil()
    {
        int usuarioId = ObtenerUsuarioIdDelToken();

        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

        if (usuario == null) return NotFound("Usuario no encontrado.");

        // Dependiendo del rol, buscamos en la tabla correspondiente
        if (usuario.Rol.NombreRol == "Cliente")
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            return Ok(new { usuario.CorreoElectronico, Rol = usuario.Rol.NombreRol, cliente?.Nombre, cliente?.Apellido, cliente?.Telefono });
        }
        else
        {
            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);
            return Ok(new { usuario.CorreoElectronico, Rol = usuario.Rol.NombreRol, empleado?.Nombre, empleado?.Apellido, empleado?.Cargo });
        }
    }

    // -----------------------------------------------------------
    // 2. ACTUALIZAR MIS DATOS (PUT /api/Perfil)
    // -----------------------------------------------------------
    [HttpPut]
    public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilDto dto)
    {
        int usuarioId = ObtenerUsuarioIdDelToken();
        var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

        if (usuario.Rol.NombreRol == "Cliente")
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            cliente.Nombre = dto.Nombre;
            cliente.Apellido = dto.Apellido;
            cliente.Telefono = dto.Telefono;
        }
        else
        {
            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);
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
        int usuarioId = ObtenerUsuarioIdDelToken();
        var usuario = await _context.Usuarios.FindAsync(usuarioId);

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