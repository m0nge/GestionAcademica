using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Materias
{
    public class DetailsModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public DetailsModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        public Materia Materia { get; set; } = default!;
        public IList<Asignacion> AsignacionesRecientes { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            // Verificar permisos (Admin puede ver todo, Docente solo sus materias)
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            var docenteId = HttpContext.Session.GetString("DocenteID");

            if (id == null || _context.Materias == null)
            {
                return NotFound();
            }

            // Cargar la materia con las relaciones que existen
            var materia = await _context.Materias
                .Include(m => m.Docente)
                    .ThenInclude(d => d.Facultad) // Si Docente tiene Facultad
                .Include(m => m.Carrera)
                    .ThenInclude(c => c.Facultad) // Si Carrera tiene Facultad
                .FirstOrDefaultAsync(m => m.MateriaID == id);

            if (materia == null)
            {
                return NotFound();
            }

            // Si es docente, verificar que sea su materia
            if (docenteRol == "Docente" && materia.DocenteID.ToString() != docenteId)
            {
                TempData["ErrorMessage"] = "No tienes permisos para ver los detalles de esta materia.";
                return RedirectToPage("/Home/Index");
            }

            Materia = materia;

            // Cargar asignaciones por separado si la navegación existe
            try
            {
                AsignacionesRecientes = await _context.Asignaciones
                    .Where(a => a.MateriaID == id)
                    .OrderByDescending(a => a.FechaCreacion)
                    .Take(5)
                    .ToListAsync();
            }
            catch
            {
                // Si no existe la tabla Asignaciones o la relación, usar lista vacía
                AsignacionesRecientes = new List<Asignacion>();
            }

            return Page();
        }
    }
}