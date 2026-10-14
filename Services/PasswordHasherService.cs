namespace voltajemoda_backend.Services;

public class PasswordHasherService : IPasswordHasherService
{
    public string HashPassword(string password)
    {
        // Genera un hash seguro con un "salt" automático
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        // Compara la contraseña en texto plano con el hash de la base de datos
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}