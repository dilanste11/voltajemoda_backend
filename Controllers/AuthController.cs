using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voltajemoda_backend.DTOs.Auth;
using voltajemoda_backend.Models;
using voltajemoda_backend.Services;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly VoltajeDbContext _context;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthController(VoltajeDbContext context, IPasswordHasherService passwordHasher, IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    [HttpPost("registro")]
    public async Task<IActionResult> RegistrarCliente([FromBody] RegistroClienteDto dto)
    {
        // 1. Validar si el correo ya existe
        if (await _context.Usuarios.AnyAsync(u => u.CorreoElectronico == dto.CorreoElectronico))
        {
            return BadRequest("El correo electrónico ya está registrado.");
        }

        // 2. Buscar el Rol "Cliente" (ID 2 según el script de base de datos)
        var rolCliente = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == "Cliente");
        if (rolCliente == null) return StatusCode(500, "Error interno: Rol 'Cliente' no encontrado en DB.");

        // 3. Crear el Usuario con la contraseña encriptada
        var nuevoUsuario = new Usuario
        {
            RolId = rolCliente.RolId,
            CorreoElectronico = dto.CorreoElectronico,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        // 4. Crear el Perfil del Cliente
        var nuevoCliente = new Cliente
        {
            Usuario = nuevoUsuario, // EF Core vincula los IDs automáticamente
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Telefono = dto.Telefono
        };

        // 5. Guardar en base de datos (se guarda Usuario y Cliente a la vez)
        _context.Clientes.Add(nuevoCliente);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Cliente registrado exitosamente." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // 1. Buscar usuario e incluir su Rol
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.CorreoElectronico == dto.CorreoElectronico);

        // 2. Validar existencia, estado activo y contraseña
        if (usuario == null || !usuario.Activo || !_passwordHasher.VerifyPassword(dto.Password, usuario.PasswordHash))
        {
            return Unauthorized("Credenciales inválidas o cuenta inactiva.");
        }

        // 3. Generar el Token JWT
        var token = _jwtService.GenerarToken(usuario, usuario.Rol.NombreRol);

        // Buscar el perfil del cliente para el nombre (si no es admin)
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuario.UsuarioId);
        string nombre = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}" : "Administrador";

        // 4. Retornar la respuesta estructurada
        var response = new AuthResponseDto
        {
            Token = token,
            NombreCompleto = nombre,
            Rol = usuario.Rol.NombreRol,
            Expiracion = DateTime.UtcNow.AddHours(2)
        };

        return Ok(response);
    }
}