using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voltajemoda_backend.DTOs.Auth;
using voltajemoda_backend.Models;
using voltajemoda_backend.Services;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
// El candado: Exige un token válido y que además pertenezca a un Administrador
[Authorize(Roles = "Administrador")]
public class UsuariosController : ControllerBase
{
    private readonly VoltajeDbContext _context;
    private readonly IPasswordHasherService _passwordHasher;

    public UsuariosController(VoltajeDbContext context, IPasswordHasherService passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("empleados/registro")]
    public async Task<IActionResult> RegistrarEmpleado([FromBody] RegistroEmpleadoDto dto)
    {
        // 1. Validar que el correo no exista ya en el sistema
        if (await _context.Usuarios.AnyAsync(u => u.CorreoElectronico == dto.CorreoElectronico))
        {
            return BadRequest("El correo electrónico ya está registrado.");
        }

        // 2. Buscar el Rol "Empleado" en la base de datos
        // Si tu base de datos solo tiene Admin y Cliente, cambia "Empleado" por "Administrador"
        var rolEmpleado = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == "Empleado");
        if (rolEmpleado == null) return StatusCode(500, "Rol 'Empleado' no encontrado en la base de datos.");

        // 3. Crear el Usuario con credenciales encriptadas
        var nuevoUsuario = new Usuario
        {
            RolId = rolEmpleado.RolId,
            CorreoElectronico = dto.CorreoElectronico,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        // 4. Crear el perfil en la tabla Empleados
        var nuevoEmpleado = new Empleado
        {
            Usuario = nuevoUsuario,
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Cargo = dto.Cargo
        };

        // 5. Guardar ambas entidades en una sola transacción
        _context.Empleados.Add(nuevoEmpleado);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Personal interno ({dto.Nombre}) registrado correctamente en el cargo de {dto.Cargo}." });
    }

    [HttpGet("clientes")]
    public async Task<IActionResult> ListarClientes()
    {
        // Hacemos un Join entre la tabla Clientes y Usuarios para traer todos los datos combinados
        var clientes = await _context.Clientes
            .Include(c => c.Usuario)
            .Select(c => new ClienteResponseDto
            {
                UsuarioId = c.UsuarioId,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                CorreoElectronico = c.Usuario.CorreoElectronico,
                Telefono = c.Telefono,
                Activo = c.Usuario.Activo,
                FechaCreacion = c.Usuario.FechaCreacion.GetValueOrDefault()
            })
            .ToListAsync();

        return Ok(clientes);
    }

    [HttpPut("clientes/{id}/estado")]
    public async Task<IActionResult> CambiarEstadoCliente(int id)
    {
        // Buscamos al usuario por su ID
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound("Usuario no encontrado.");

        // Verificamos que el usuario que intentan bloquear sea realmente un Cliente (RolId = 2)
        // para evitar que un administrador se bloquee a sí mismo o a un empleado por error.
        var rolCliente = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == "Cliente");
        if (usuario.RolId != rolCliente?.RolId)
            return BadRequest("Solo se puede cambiar el estado de cuentas de clientes mediante este endpoint.");

        // Alternamos el estado booleano (Si es true pasa a false, si es false pasa a true)
        usuario.Activo = !usuario.Activo;

        await _context.SaveChangesAsync();

        string estadoActual = usuario.Activo ? "Activado" : "Bloqueado (Inactivo)";
        return Ok(new { mensaje = $"El estado del cliente ha sido actualizado a: {estadoActual}." });
    }

    [HttpGet("empleados")]
    public async Task<IActionResult> ListarEmpleados()
    {
        // Hacemos el Join con la tabla Empleados
        var empleados = await _context.Empleados
            .Include(e => e.Usuario)
            .Select(e => new EmpleadoResponseDto
            {
                UsuarioId = e.UsuarioId,
                Nombre = e.Nombre,
                Apellido = e.Apellido,
                CorreoElectronico = e.Usuario.CorreoElectronico,
                Cargo = e.Cargo,
                Activo = e.Usuario.Activo,
                // Usamos GetValueOrDefault() para evitar el error de nulos que tuvimos antes
                FechaCreacion = e.Usuario.FechaCreacion.GetValueOrDefault()
            })
            .ToListAsync();

        return Ok(empleados);
    }

    [HttpPut("empleados/{id}/estado")]
    public async Task<IActionResult> CambiarEstadoEmpleado(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound("Usuario no encontrado.");

        // Verificamos que NO sea un cliente (debe ser Empleado o Admin)
        var rolCliente = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == "Cliente");
        if (usuario.RolId == rolCliente?.RolId)
            return BadRequest("Utilice el endpoint de clientes para modificar cuentas de compradores.");

        // PROTECCIÓN CRÍTICA: Evitar auto-bloqueo
        // Extraemos el ID del administrador que está haciendo la petición desde su token JWT
        var currentUserIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);

        if (currentUserIdClaim != null && currentUserIdClaim.Value == id.ToString())
        {
            return BadRequest(new { error = true, mensaje = "Por seguridad, no puedes bloquear tu propia cuenta de administrador." });
        }

        // Alternamos el estado
        usuario.Activo = !usuario.Activo;
        await _context.SaveChangesAsync();

        string estadoActual = usuario.Activo ? "Activado" : "Bloqueado (Inactivo)";
        return Ok(new { mensaje = $"El estado del personal interno ha sido actualizado a: {estadoActual}." });
    }

    [HttpPut("{id}/rol")]
    public async Task<IActionResult> CambiarRolUsuario(int id, [FromBody] CambiarRolDto dto)
    {
        // 1. Buscar el usuario y su rol actual
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.UsuarioId == id);

        if (usuario == null) return NotFound("Usuario no encontrado.");

        // 2. Buscar el nuevo rol solicitado
        var nuevoRol = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == dto.NuevoRol);
        if (nuevoRol == null) return BadRequest("El rol especificado no existe en el sistema.");

        if (usuario.RolId == nuevoRol.RolId)
            return BadRequest("El usuario ya tiene asignado este rol.");

        // 3. PROTECCIÓN CRÍTICA: Evitar quitarse permisos a sí mismo
        var currentUserIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (currentUserIdClaim != null && currentUserIdClaim.Value == id.ToString())
        {
            return BadRequest(new { error = true, mensaje = "Por seguridad, no puedes cambiar tu propio rol." });
        }

        string rolActual = usuario.Rol.NombreRol;

        // 4. Lógica de Migración: De Cliente a Empleado/Admin
        if (rolActual == "Cliente" && (dto.NuevoRol == "Empleado" || dto.NuevoRol == "Administrador"))
        {
            if (string.IsNullOrWhiteSpace(dto.Cargo))
                return BadRequest("Para promover a un cliente, debes enviar obligatoriamente el campo 'cargo'.");

            var perfilCliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == id);
            if (perfilCliente != null)
            {
                var nuevoEmpleado = new Empleado
                {
                    UsuarioId = id,
                    Nombre = perfilCliente.Nombre,
                    Apellido = perfilCliente.Apellido,
                    Cargo = dto.Cargo
                };
                _context.Empleados.Add(nuevoEmpleado);
                _context.Clientes.Remove(perfilCliente);
            }
        }
        // 5. Lógica de Migración: De Empleado/Admin a Cliente
        else if ((rolActual == "Empleado" || rolActual == "Administrador") && dto.NuevoRol == "Cliente")
        {
            var perfilEmpleado = await _context.Empleados.FirstOrDefaultAsync(e => e.UsuarioId == id);
            if (perfilEmpleado != null)
            {
                var nuevoCliente = new Cliente
                {
                    UsuarioId = id,
                    Nombre = perfilEmpleado.Nombre,
                    Apellido = perfilEmpleado.Apellido,
                    Telefono = null // Queda nulo hasta que el cliente lo actualice en su perfil
                };
                _context.Clientes.Add(nuevoCliente);
                _context.Empleados.Remove(perfilEmpleado);
            }
        }

        // 6. Actualizar la tabla principal y guardar
        usuario.RolId = nuevoRol.RolId;
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Operación exitosa. El usuario ahora tiene el rol de: {dto.NuevoRol}." });
    }
}