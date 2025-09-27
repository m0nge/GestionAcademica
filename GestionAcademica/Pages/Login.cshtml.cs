using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using GestionAcademica.Models;
using GestionAcademica.Services;

namespace GestionAcademica.Pages
{
    public class LoginModel : PageModel
    {
        private readonly GestionAcademicaContext _context;
        private readonly ITwoFactorService _twoFactorService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            GestionAcademicaContext context,
            ITwoFactorService twoFactorService,
            ILogger<LoginModel> logger)
        {
            _context = context;
            _twoFactorService = twoFactorService;
            _logger = logger;
        }

        [BindProperty]
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Debe seleccionar un tipo de usuario")]
        public string TipoUsuario { get; set; } = string.Empty;

        public void OnGet()
        {
            _logger.LogInformation("📋 Página Login cargada");

            // Verificar si ya está logueado
            if (HttpContext.Session.GetString("DocenteLogueado") != null)
            {
                _logger.LogInformation("👤 Usuario ya logueado, redirigiendo a Home");
                Response.Redirect("/Home/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("🚀 INICIO LOGIN POST - Email: {Email}, TipoUsuario: {TipoUsuario}", Email, TipoUsuario);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("❌ ModelState inválido");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("❌ Error de validación: {Error}", error.ErrorMessage);
                }
                return Page();
            }

            var clientIP = HttpContext.Connection.RemoteIpAddress?.ToString();
            _logger.LogInformation("🌐 IP del cliente: {ClientIP}", clientIP);

            try
            {
                _logger.LogInformation("🔍 Buscando docente con email: {Email}", Email);

                // Buscar el docente en la base de datos
                var docente = await _context.Docentes
                    .Include(d => d.Facultad)
                    .FirstOrDefaultAsync(d => d.Email == Email && d.Activo);

                if (docente == null)
                {
                    _logger.LogWarning("❌ No se encontró docente con email {Email} o está inactivo", Email);
                    TempData["ErrorMessage"] = "No se encontró un usuario con este email o la cuenta está inactiva.";
                    return Page();
                }

                _logger.LogInformation("✅ Docente encontrado: {Usuario}, Rol: {Rol}, Email: {Email}", docente.Email, docente.Rol, docente.Email);

                // Verificar si la cuenta está bloqueada
                bool isLocked = await _twoFactorService.IsAccountLockedAsync(docente.DocenteID);
                _logger.LogInformation("🔒 Cuenta bloqueada: {IsLocked}", isLocked);

                if (isLocked)
                {
                    var minutesLeft = (docente.BloqueadoHasta!.Value - DateTime.Now).Minutes;
                    TempData["ErrorMessage"] = $"Cuenta bloqueada por intentos fallidos. Inténtalo en {minutesLeft} minutos.";
                    _logger.LogWarning("🔒 Cuenta bloqueada para {Email}, minutos restantes: {Minutes}", Email, minutesLeft);
                    return Page();
                }

                // Verificar si el docente tiene el rol correcto
                _logger.LogInformation("🎭 Verificando rol - Esperado: {TipoUsuario}, Real: {DocenteRol}", TipoUsuario, docente.Rol);

                if (docente.Rol != TipoUsuario)
                {
                    _logger.LogWarning("❌ Rol incorrecto para {Email} - Esperado: {TipoUsuario}, Real: {DocenteRol}", Email, TipoUsuario, docente.Rol);
                    TempData["ErrorMessage"] = $"Este usuario no tiene permisos de {TipoUsuario}.";
                    await _twoFactorService.RegisterFailedAttemptAsync(docente.DocenteID, clientIP);
                    return Page();
                }

                // Verificar contraseña
                _logger.LogInformation("🔑 Verificando contraseña para {Email}", Email);
                bool passwordValid = false;

                // TEMPORAL: Compatibilidad con contraseñas antiguas
                if (docente.Contraseña == Password)
                {
                    _logger.LogInformation("🔑 Contraseña antigua encontrada, actualizando a hash");
                    docente.Contraseña = _twoFactorService.HashPassword(Password);
                    await _context.SaveChangesAsync();
                    passwordValid = true;
                }
                else
                {
                    // Verificar contraseña hasheada
                    passwordValid = _twoFactorService.VerifyPassword(Password, docente.Contraseña);
                    _logger.LogInformation("🔑 Verificación de contraseña hash: {IsValid}", passwordValid);
                }

                if (!passwordValid)
                {
                    _logger.LogWarning("❌ Contraseña incorrecta para {Email}", Email);
                    TempData["ErrorMessage"] = "Contraseña incorrecta.";
                    await _twoFactorService.RegisterFailedAttemptAsync(docente.DocenteID, clientIP);
                    return Page();
                }

                _logger.LogInformation("✅ Credenciales válidas para {Email}", Email);

                // Verificar si tiene 2FA habilitado
                _logger.LogInformation("🔐 TwoFactorEnabled: {TwoFactorEnabled}", docente.TwoFactorEnabled);

                if (docente.TwoFactorEnabled)
                {
                    _logger.LogInformation("🔐 Iniciando proceso 2FA para {Email}", Email);

                    // Enviar código 2FA
                    bool codigoEnviado = await _twoFactorService.SendTwoFactorCodeAsync(docente);
                    _logger.LogInformation("📧 Código 2FA enviado: {CodigoEnviado}", codigoEnviado);

                    if (!codigoEnviado)
                    {
                        _logger.LogError("❌ Error enviando código 2FA para {Email}", Email);
                        TempData["ErrorMessage"] = "Error enviando código de verificación. Contacta al administrador.";
                        return Page();
                    }

                    // Crear sesión temporal para 2FA
                    _logger.LogInformation("💾 Creando sesión temporal 2FA para {Email}", Email);
                    HttpContext.Session.SetString("TwoFactorPending", "true");
                    HttpContext.Session.SetString("TempDocenteID", docente.DocenteID.ToString());
                    HttpContext.Session.SetString("TempDocenteEmail", docente.Email);
                    HttpContext.Session.SetString("TempDocenteNombre", docente.NombreCompleto);
                    HttpContext.Session.SetString("TempDocenteRol", docente.Rol);
                    HttpContext.Session.SetString("TempDocenteFacultadID", docente.FacultadID?.ToString() ?? "");

                    TempData["InfoMessage"] = "Se ha enviado un código de verificación a tu email.";

                    _logger.LogInformation("🔄 Redirigiendo a TwoFactor para {Email}", Email);
                    return RedirectToPage("/TwoFactor");
                }
                else
                {
                    _logger.LogInformation("✅ 2FA deshabilitado, completando login directamente para {Email}", Email);

                    // Login directo sin 2FA
                    await CompletarLoginAsync(docente, clientIP);
                    return RedirectToPage("/Home/Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 ERROR GENERAL en login para {Email}", Email);
                TempData["ErrorMessage"] = "Error al conectar con la base de datos. Intente nuevamente.";
                return Page();
            }
        }

        private async Task CompletarLoginAsync(Docente docente, string? clientIP)
        {
            _logger.LogInformation("🎉 Completando login para {Email}", docente.Email);

            // Registrar login exitoso
            await _twoFactorService.RegisterSuccessfulLoginAsync(docente.DocenteID, clientIP);

            // Crear sesión completa
            HttpContext.Session.SetString("DocenteLogueado", "true");
            HttpContext.Session.SetString("DocenteID", docente.DocenteID.ToString());
            HttpContext.Session.SetString("DocenteNombre", docente.NombreCompleto);
            HttpContext.Session.SetString("DocenteEmail", docente.Email);
            HttpContext.Session.SetString("DocenteRol", docente.Rol);
            HttpContext.Session.SetString("DocenteFacultadID", docente.FacultadID?.ToString() ?? "");

            // Limpiar sesiones temporales si existen
            HttpContext.Session.Remove("TwoFactorPending");
            HttpContext.Session.Remove("TempDocenteID");
            HttpContext.Session.Remove("TempDocenteEmail");
            HttpContext.Session.Remove("TempDocenteNombre");
            HttpContext.Session.Remove("TempDocenteRol");
            HttpContext.Session.Remove("TempDocenteFacultadID");

            TempData["SuccessMessage"] = $"¡Bienvenido, {docente.Nombres}! Sesión iniciada como {docente.Rol}.";

            _logger.LogInformation("✅ Login completado exitosamente para {Email}", docente.Email);
        }
    }
}