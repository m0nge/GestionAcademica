using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Asignaciones
{
    public class DeleteModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public DeleteModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Asignacion Asignacion { get; set; } = default!;

        public Materia Materia { get; set; } = default!;
        public int TotalCalificaciones { get; set; }
        public bool TieneCalificaciones { get; set; }

        [BindProperty(SupportsGet = true)]
        public int MateriaId { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            if (id == null)
            {
                return NotFound();
            }

            // Cargar la asignación con todas sus relaciones
            Asignacion = await _context.Asignaciones
                .Include(a => a.Materia)
                    .ThenInclude(m => m.Docente)
                .Include(a => a.TipoAsignacion)
                .FirstOrDefaultAsync(a => a.AsignacionID == id);

            if (Asignacion == null)
            {
                return NotFound();
            }

            // Obtener información del docente logueado
            var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";

            // Verificar permisos: Solo el docente dueño puede eliminar sus asignaciones
            if (docenteRol != "Administrador" && Asignacion.Materia.DocenteID != docenteId)
            {
                TempData["ErrorMessage"] = "No tienes permisos para eliminar esta asignación.";
                return RedirectToPage("/Materias/Index");
            }

            // Cargar materia
            Materia = Asignacion.Materia;
            MateriaId = Asignacion.MateriaID;

            // Verificar si tiene calificaciones asociadas
            TotalCalificaciones = await _context.Calificaciones
                .Where(c => c.AsignacionID == Asignacion.AsignacionID)
                .CountAsync();

            TieneCalificaciones = TotalCalificaciones > 0;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            if (id == null)
            {
                return NotFound();
            }

            // Obtener información del docente logueado
            var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";

            // Cargar la asignación con validaciones
            var asignacion = await _context.Asignaciones
                .Include(a => a.Materia)
                .FirstOrDefaultAsync(a => a.AsignacionID == id);

            if (asignacion == null)
            {
                TempData["ErrorMessage"] = "Asignación no encontrada.";
                return RedirectToPage("/Materias/Index");
            }

            // Verificar permisos
            if (docenteRol != "Administrador" && asignacion.Materia.DocenteID != docenteId)
            {
                TempData["ErrorMessage"] = "No tienes permisos para eliminar esta asignación.";
                return RedirectToPage("/Materias/Index");
            }

            try
            {
                // Verificar si tiene calificaciones
                var tieneCalificaciones = await _context.Calificaciones
                    .AnyAsync(c => c.AsignacionID == asignacion.AsignacionID);

                if (tieneCalificaciones)
                {
                    // Si tiene calificaciones, eliminarlas primero
                    var calificaciones = await _context.Calificaciones
                        .Where(c => c.AsignacionID == asignacion.AsignacionID)
                        .ToListAsync();

                    _context.Calificaciones.RemoveRange(calificaciones);
                }

                // Eliminar la asignación
                _context.Asignaciones.Remove(asignacion);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Asignación '{asignacion.Nombre}' eliminada exitosamente.";
                return RedirectToPage("./Index", new { materiaId = asignacion.MateriaID });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la asignación. Intente nuevamente.";
                return RedirectToPage("./Index", new { materiaId = asignacion.MateriaID });
            }
        }
    }
}