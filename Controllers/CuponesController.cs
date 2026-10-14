using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using voltajemoda_backend.DTOs.Marketing;
using voltajemoda_backend.Models;

namespace voltajemoda_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Cliente")]
public class CuponesController : ControllerBase
{
    private readonly VoltajeDbContext _context;

    public CuponesController(VoltajeDbContext context)
    {
        _context = context;
    }

    [HttpPost("validar")]
    public async Task<IActionResult> ValidarCupon([FromBody] ValidarCuponDto dto)
    {
        var cupon = await _context.Cupones
            .FirstOrDefaultAsync(c => c.Codigo.ToLower() == dto.Codigo.ToLower());

        if (cupon == null)
            return NotFound("El cupón ingresado no existe.");

        if (!cupon.Activo)
            return BadRequest("Este cupón ha sido desactivado.");

        var hoy = DateTime.Now;
        if (hoy < cupon.ValidoDesde)
            return BadRequest("Este cupón aún no está vigente.");

        if (hoy > cupon.ValidoHasta)
            return BadRequest("Este cupón ya ha caducado.");

        // Si pasa todas las validaciones, le devolvemos la info al frontend
        return Ok(new
        {
            mensaje = "Cupón válido.",
            cuponId = cupon.CuponId, // El frontend guardará este ID para enviarlo al Checkout
            codigo = cupon.Codigo,
            porcentajeDescuento = cupon.Descuento
        });
    }
}