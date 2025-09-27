using GestionAcademica.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GestionAcademica.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly GestionAcademicaContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<TwoFactorService> _logger;

        private const int MAX_FAILED_ATTEMPTS = 5;
        private const int LOCKOUT_MINUTES = 30;
        private const int CODE_EXPIRY_MINUTES = 5;

        public TwoFactorService(
            GestionAcademicaContext context,
            EmailService emailService,
            ILogger<TwoFactorService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public string GenerateSecureCode()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);

            // Convertir a número de 6 dígitos
            int code = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
            return code.ToString("D6");
        }

        public async Task<bool> SendTwoFactorCodeAsync(Docente docente)
        {
            try
            {
                var code = GenerateSecureCode();
                var expiry = DateTime.Now.AddMinutes(CODE_EXPIRY_MINUTES);

                // Actualizar docente con el nuevo código
                docente.TwoFactorCode = code;
                docente.TwoFactorCodeExpiry = expiry;

                await _context.SaveChangesAsync();

                // Enviar email
                var success = await _emailService.SendTwoFactorCodeAsync(
                    docente.Email,
                    code,
                    docente.NombreCompleto
                );

                if (success)
                {
                    _logger.LogInformation($"Código 2FA enviado a docente {docente.DocenteID}");
                }
                else
                {
                    _logger.LogError($"Error enviando código 2FA a docente {docente.DocenteID}");
                    // Limpiar código si falló el envío
                    docente.TwoFactorCode = null;
                    docente.TwoFactorCodeExpiry = null;
                    await _context.SaveChangesAsync();
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en SendTwoFactorCodeAsync para docente {docente.DocenteID}");
                return false;
            }
        }

        public async Task<bool> ValidateCodeAsync(int docenteId, string code)
        {
            try
            {
                var docente = await _context.Docentes.FindAsync(docenteId);
                if (docente == null) return false;

                // Verificar si tiene código y no ha expirado
                if (string.IsNullOrEmpty(docente.TwoFactorCode) ||
                    !docente.TwoFactorCodeExpiry.HasValue ||
                    docente.TwoFactorCodeExpiry.Value <= DateTime.Now)
                {
                    return false;
                }

                // Verificar código
                bool isValid = docente.TwoFactorCode == code;

                if (isValid)
                {
                    // Limpiar código usado
                    await ClearCodeAsync(docenteId);
                    _logger.LogInformation($"Código 2FA válido para docente {docenteId}");
                }
                else
                {
                    _logger.LogWarning($"Código 2FA inválido para docente {docenteId}");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validando código 2FA para docente {docenteId}");
                return false;
            }
        }

        public async Task ClearCodeAsync(int docenteId)
        {
            try
            {
                var docente = await _context.Docentes.FindAsync(docenteId);
                if (docente != null)
                {
                    docente.TwoFactorCode = null;
                    docente.TwoFactorCodeExpiry = null;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error limpiando código 2FA para docente {docenteId}");
            }
        }

        public async Task<bool> IsAccountLockedAsync(int docenteId)
        {
            try
            {
                var docente = await _context.Docentes.FindAsync(docenteId);
                return docente?.EstaBloqueado ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verificando bloqueo para docente {docenteId}");
                return false;
            }
        }

        public async Task RegisterFailedAttemptAsync(int docenteId, string? ipAddress)
        {
            try
            {
                var docente = await _context.Docentes.FindAsync(docenteId);
                if (docente != null)
                {
                    docente.IntentosFallidos++;
                    docente.UltimaIP = ipAddress;

                    // Bloquear si excede intentos máximos
                    if (docente.IntentosFallidos >= MAX_FAILED_ATTEMPTS)
                    {
                        docente.BloqueadoHasta = DateTime.Now.AddMinutes(LOCKOUT_MINUTES);
                        _logger.LogWarning($"Cuenta bloqueada para docente {docenteId} por {LOCKOUT_MINUTES} minutos");
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registrando intento fallido para docente {docenteId}");
            }
        }

        public async Task RegisterSuccessfulLoginAsync(int docenteId, string? ipAddress)
        {
            try
            {
                var docente = await _context.Docentes.FindAsync(docenteId);
                if (docente != null)
                {
                    docente.IntentosFallidos = 0;
                    docente.BloqueadoHasta = null;
                    docente.UltimoAcceso = DateTime.Now;
                    docente.UltimaIP = ipAddress;

                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Login exitoso para docente {docenteId} desde IP {ipAddress}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registrando login exitoso para docente {docenteId}");
            }
        }

        // Métodos para hashear contraseñas (CRÍTICO para seguridad)
        public string HashPassword(string password)
        {
            // Salt específico para tu aplicación - cámbialo por algo único
            const string SALT = "GestionAcademica2024!@#$";

            using var sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + SALT));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                var hashOfInput = HashPassword(password);
                return hashOfInput == hashedPassword;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando contraseña");
                return false;
            }
        }
    }
}