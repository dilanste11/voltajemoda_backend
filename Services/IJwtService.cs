using voltajemoda_backend.Models;

namespace voltajemoda_backend.Services;

public interface IJwtService
{  
    string GenerarToken(Usuario usuario, string nombreRol);
}