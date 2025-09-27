using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;
using System.ComponentModel.DataAnnotations;

namespace GestionAcademica.Pages.Configuracion
{
    public class IndexModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public IndexModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CambiarContrasenaModel CambiarContrasena { get; set; } = new();

        public string DocenteNombre { get; set; } = "";
        public string DocenteEmail { get; set; } = "";
        public string DocenteRol { get; set; } = "";
        public DateTime FechaRegistro { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            // Obtener información del docente logueado
            var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");

            var docente = await _context.Docentes.FindAsync(docenteId);
            if (docente == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado.";
                return RedirectToPage("/Login");
            }

            // Cargar información del perfil
            DocenteNombre = docente.NombreCompleto;
            DocenteEmail = docente.Email;
            DocenteRol = docente.Rol;
            FechaRegistro = docente.FechaRegistro;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            // Recargar información del perfil para mostrar en caso de error
            await OnGetAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");
                var docente = await _context.Docentes.FindAsync(docenteId);

                if (docente == null)
                {
                    TempData["ErrorMessage"] = "Usuario no encontrado.";
                    return Page();
                }

                // Verificar contraseña actual
                if (docente.Contraseña != CambiarContrasena.ContrasenaActual)
                {
                    ModelState.AddModelError("CambiarContrasena.ContrasenaActual", "La contraseña actual es incorrecta.");
                    return Page();
                }

                // Verificar que la nueva contraseña sea diferente
                if (CambiarContrasena.ContrasenaActual == CambiarContrasena.NuevaContrasena)
                {
                    ModelState.AddModelError("CambiarContrasena.NuevaContrasena", "La nueva contraseña debe ser diferente a la actual.");
                    return Page();
                }

                // Actualizar contraseña
                docente.Contraseña = CambiarContrasena.NuevaContrasena;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Contraseña actualizada exitosamente.";

                // Limpiar el formulario
                CambiarContrasena = new CambiarContrasenaModel();

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar la contraseña. Intente nuevamente.";
                return Page();
            }
        }
    }

    public class CambiarContrasenaModel
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        [Display(Name = "Contraseña Actual")]
        public string ContrasenaActual { get; set; } = "";

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 255 caracteres")]
        [Display(Name = "Nueva Contraseña")]
        public string NuevaContrasena { get; set; } = "";

        [Required(ErrorMessage = "Debe confirmar la nueva contraseña")]
        [Compare("NuevaContrasena", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar Nueva Contraseña")]
        public string ConfirmarContrasena { get; set; } = "";
    }
}