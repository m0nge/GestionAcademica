using GestionAcademica.Models;

namespace GestionAcademica.Services
{
    public interface ITwoFactorService
    {
        string GenerateSecureCode();
        Task<bool> SendTwoFactorCodeAsync(Docente docente);
        Task<bool> ValidateCodeAsync(int docenteId, string code);
        Task ClearCodeAsync(int docenteId);
        Task<bool> IsAccountLockedAsync(int docenteId);
        Task RegisterFailedAttemptAsync(int docenteId, string? ipAddress);
        Task RegisterSuccessfulLoginAsync(int docenteId, string? ipAddress);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}