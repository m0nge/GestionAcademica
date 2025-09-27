using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using GestionAcademica.Services;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages
{
    public class TwoFactorModel : PageModel
    {
        private readonly ITwoFactorService _twoFactorService;
        private readonly GestionAcademicaContext _context;
        private readonly ILogger<TwoFactorModel> _logger;

        public TwoFactorModel(
            ITwoFactorService twoFactorService,
            GestionAcademicaContext context,
            ILogger<TwoFactorModel> logger)
        {
            _twoFactorService = twoFactorService;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener 6 dígitos")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "El código debe contener solo números")]
        public string VerificationCode { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar que hay una sesión 2FA pendiente
            if (HttpContext.Session.GetString("TwoFactorPending") != "true")
            {
                TempData["ErrorMessage"] = "No hay verificación pendiente. Inicia sesión nuevamente.";
                return RedirectToPage("/Login");
            }

            // Cargar información del usuario
            UserName = HttpContext.Session.GetString("TempDocenteNombre") ?? "Usuario";
            UserEmail = HttpContext.Session.GetString("TempDocenteEmail") ?? "";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool resend = false)
        {
            // Verificar sesión 2FA
            if (HttpContext.Session.GetString("TwoFactorPending") != "true")
            {
                TempData["ErrorMessage"] = "Sesión expirada. Inicia sesión nuevamente.";
                return RedirectToPage("/Login");
            }

            // Cargar información del usuario
            UserName = HttpContext.Session.GetString("TempDocenteNombre") ?? "Usuario";
            UserEmail = HttpContext.Session.GetString("TempDocenteEmail") ?? "";

            var docenteIdStr = HttpContext.Session.GetString("TempDocenteID");
            if (!int.TryParse(docenteIdStr, out int docenteId))
            {
                TempData["ErrorMessage"] = "Error en la sesión. Inicia sesión nuevamente.";
                return RedirectToPage("/Login");
            }

            var clientIP = HttpContext.Connection.RemoteIpAddress?.ToString();

            try
            {
                // Si es reenvío de código
                if (resend)
                {
                    var docente = await _context.Docentes.FindAsync(docenteId);
                    if (docente == null)
                    {
                        TempData["ErrorMessage"] = "Usuario no encontrado.";
                        return RedirectToPage("/Login");
                    }

                    bool codigoEnviado = await _twoFactorService.SendTwoFactorCodeAsync(docente);
                    if (codigoEnviado)
                    {
                        TempData["InfoMessage"] = "Nuevo código enviado a tu email.";
                        _logger.LogInformation($"Código 2FA reenviado a docente {docenteId}");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error enviando el código. Intenta más tarde.";
                    }

                    return Page();
                }

                // Validar el modelo
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // Validar código 2FA
                bool codigoValido = await _twoFactorService.ValidateCodeAsync(docenteId, VerificationCode);

                if (!codigoValido)
                {
                    TempData["ErrorMessage"] = "Código incorrecto o expirado. Intenta nuevamente.";
                    await _twoFactorService.RegisterFailedAttemptAsync(docenteId, clientIP);

                    _logger.LogWarning($"Código 2FA inválido para docente {docenteId} desde IP {clientIP}");

                    // Limpiar el input
                    VerificationCode = string.Empty;
                    return Page();
                }

                // Código válido - Completar login
                var docenteCompleto = await _context.Docentes
                    .Include(d => d.Facultad)
                    .FirstOrDefaultAsync(d => d.DocenteID == docenteId);

                if (docenteCompleto == null)
                {
                    TempData["ErrorMessage"] = "Error cargando usuario.";
                    return RedirectToPage("/Login");
                }

                // Completar el login
                await CompletarLoginAsync(docenteCompleto, clientIP);

                _logger.LogInformation($"Login 2FA completado exitosamente para docente {docenteId}");
                TempData["SuccessMessage"] = "Verificación exitosa. ¡Bienvenido!";

                return RedirectToPage("/Home/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en verificación 2FA para docente {docenteId}");
                TempData["ErrorMessage"] = "Error en la verificación. Intenta nuevamente.";
                return Page();
            }
        }

        private async Task CompletarLoginAsync(Docente docente, string? clientIP)
        {
            // Registrar login exitoso
            await _twoFactorService.RegisterSuccessfulLoginAsync(docente.DocenteID, clientIP);

            // Crear sesión completa
            HttpContext.Session.SetString("DocenteLogueado", "true");
            HttpContext.Session.SetString("DocenteID", docente.DocenteID.ToString());
            HttpContext.Session.SetString("DocenteNombre", docente.NombreCompleto);
            HttpContext.Session.SetString("DocenteEmail", docente.Email);
            HttpContext.Session.SetString("DocenteRol", docente.Rol);
            HttpContext.Session.SetString("DocenteFacultadID", docente.FacultadID?.ToString() ?? "");

            // Limpiar sesiones temporales de 2FA
            HttpContext.Session.Remove("TwoFactorPending");
            HttpContext.Session.Remove("TempDocenteID");
            HttpContext.Session.Remove("TempDocenteEmail");
            HttpContext.Session.Remove("TempDocenteNombre");
            HttpContext.Session.Remove("TempDocenteRol");
            HttpContext.Session.Remove("TempDocenteFacultadID");
        }
    }
}