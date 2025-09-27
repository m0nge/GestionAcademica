using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Docentes
{
    public class IndexModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public IndexModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        public IList<Docente> Docentes { get; set; } = default!;
        public string DocenteRol { get; set; } = "";
        public bool EsAdministrador { get; set; } = false;

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            // Verificar que sea administrador
            DocenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            EsAdministrador = DocenteRol == "Administrador";

            if (!EsAdministrador)
            {
                TempData["ErrorMessage"] = "No tienes permisos para acceder a la gestión de docentes.";
                return RedirectToPage("/Home/Index");
            }

            // Cargar docentes con información de facultad
            if (_context.Docentes != null)
            {
                Docentes = await _context.Docentes
                    .Include(d => d.Facultad)
                    .OrderBy(d => d.Facultad.Nombre)      // Primero por facultad
                    .ThenBy(d => d.Apellidos)             // Luego por apellidos
                    .ThenBy(d => d.Nombres)               // Finalmente por nombres
                    .ToListAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostToggleEstadoAsync(int id)
        {
            // Verificar permisos
            if (HttpContext.Session.GetString("DocenteRol") != "Administrador")
            {
                TempData["ErrorMessage"] = "No tienes permisos para realizar esta acción.";
                return RedirectToPage();
            }

            var docente = await _context.Docentes.FindAsync(id);
            if (docente != null)
            {
                docente.Activo = !docente.Activo;
                await _context.SaveChangesAsync();

                string estado = docente.Activo ? "activado" : "desactivado";
                TempData["SuccessMessage"] = $"Docente {docente.NombreCompleto} {estado} correctamente.";
            }

            return RedirectToPage();
        }
    }
}