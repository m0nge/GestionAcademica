using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Materias
{
    public class DeleteModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public DeleteModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Materia Materia { get; set; } = default!;
        public bool TieneAsignaciones { get; set; }
        public int TotalAsignaciones { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Verificar sesión y permisos de administrador
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            if (docenteRol != "Administrador")
            {
                TempData["ErrorMessage"] = "No tienes permisos para eliminar materias.";
                return RedirectToPage("./Index");
            }

            if (id == null || _context.Materias == null)
            {
                return NotFound();
            }

            var materia = await _context.Materias
                .Include(m => m.Docente)
                .Include(m => m.Asignaciones)
                .FirstOrDefaultAsync(m => m.MateriaID == id);

            if (materia == null)
            {
                return NotFound();
            }

            Materia = materia;
            TotalAsignaciones = materia.Asignaciones?.Count ?? 0;
            TieneAsignaciones = TotalAsignaciones > 0;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // Verificar permisos nuevamente
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            if (docenteRol != "Administrador")
            {
                return Forbid();
            }

            if (id == null || _context.Materias == null)
            {
                return NotFound();
            }

            var materia = await _context.Materias
                .Include(m => m.Asignaciones)
                .FirstOrDefaultAsync(m => m.MateriaID == id);

            if (materia != null)
            {
                try
                {
                    // Verificar si tiene asignaciones
                    var tieneAsignaciones = materia.Asignaciones?.Any() ?? false;

                    if (tieneAsignaciones)
                    {
                        TempData["ErrorMessage"] = $"No se puede eliminar la materia '{materia.Nombre}' porque tiene {materia.Asignaciones.Count} asignaciones asociadas. Elimina primero las asignaciones o desactiva la materia.";
                        return RedirectToPage("./Index");
                    }

                    Materia = materia;
                    _context.Materias.Remove(materia);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Materia '{materia.Nombre}' eliminada exitosamente.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error al eliminar la materia. Puede que tenga datos relacionados.";
                }
            }

            return RedirectToPage("./Index");
        }
    }
}