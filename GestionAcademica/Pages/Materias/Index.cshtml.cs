using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Materias
{
    public class IndexModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public IndexModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        public IList<Materia> Materias { get; set; } = default!;
        public string DocenteRol { get; set; } = "";
        public bool EsAdministrador { get; set; } = false;

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            // Obtener información del usuario logueado
            DocenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            EsAdministrador = DocenteRol == "Administrador";

            // Cargar materias según el rol
            if (_context.Materias != null)
            {
                if (EsAdministrador)
                {
                    // ADMINISTRADOR: Ve todas las materias
                    Materias = await _context.Materias
                        .Include(m => m.Docente)
                        .Include(m => m.Asignaciones)
                        .Include(m => m.Carrera)
                            .ThenInclude(c => c.Facultad)
                        .OrderBy(m => m.Codigo)
                        .ToListAsync();
                }
                else
                {
                    // DOCENTE: Solo ve SUS materias asignadas
                    var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");

                    Materias = await _context.Materias
                        .Include(m => m.Docente)
                        .Include(m => m.Asignaciones)
                        .Include(m => m.Carrera)
                            .ThenInclude(c => c.Facultad)
                        .Where(m => m.DocenteID == docenteId)  // Solo SUS materias
                        .OrderBy(m => m.Codigo)
                        .ToListAsync();
                }
            }

            return Page();
        }
    


        public async Task<IActionResult> OnPostToggleEstadoAsync(int id)
        {
            // Solo ADMINISTRADORES pueden cambiar el estado
            if (HttpContext.Session.GetString("DocenteRol") != "Administrador")
            {
                TempData["ErrorMessage"] = "No tienes permisos para realizar esta acción.";
                return RedirectToPage();
            }

            var materia = await _context.Materias.FindAsync(id);
            if (materia != null)
            {
                materia.Activa = !materia.Activa;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Estado de la materia {materia.Nombre} actualizado correctamente.";
            }

            return RedirectToPage();
        }
    }
}