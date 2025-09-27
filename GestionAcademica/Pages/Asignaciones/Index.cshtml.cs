using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Asignaciones
{
    public class IndexModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public IndexModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        public IList<Asignacion> Asignaciones { get; set; } = default!;
        public Materia Materia { get; set; } = default!;
        public decimal PorcentajeTotal { get; set; }
        public bool PorcentajeCompleto { get; set; }

        [BindProperty(SupportsGet = true)]
        public int MateriaId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            // Obtener información del docente logueado
            var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";

            // Verificar que la materia existe
            Materia = await _context.Materias
                .Include(m => m.Docente)
                .Include(m => m.Carrera)
                .FirstOrDefaultAsync(m => m.MateriaID == MateriaId);

            if (Materia == null)
            {
                TempData["ErrorMessage"] = "Materia no encontrada.";
                return RedirectToPage("/Materias/Index");
            }

            // Verificar permisos: Solo el docente dueño puede gestionar sus asignaciones
            if (docenteRol != "Administrador" && Materia.DocenteID != docenteId)
            {
                TempData["ErrorMessage"] = "No tienes permisos para gestionar las asignaciones de esta materia.";
                return RedirectToPage("/Materias/Index");
            }

            // Cargar asignaciones de la materia
            Asignaciones = await _context.Asignaciones
                .Include(a => a.TipoAsignacion)
                .Where(a => a.MateriaID == MateriaId)
                .OrderBy(a => a.FechaCreacion)
                .ToListAsync();

            // Calcular porcentaje total
            PorcentajeTotal = Asignaciones.Sum(a => a.Porcentaje);
            PorcentajeCompleto = PorcentajeTotal == 100;

            return Page();
        }

        public async Task<IActionResult> OnPostToggleEstadoAsync(int id)
        {
            // Verificar permisos
            var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";

            var asignacion = await _context.Asignaciones
                .Include(a => a.Materia)
                .FirstOrDefaultAsync(a => a.AsignacionID == id);

            if (asignacion == null)
            {
                TempData["ErrorMessage"] = "Asignación no encontrada.";
                return RedirectToPage();
            }

            // Verificar que el docente es dueño de la materia
            if (docenteRol != "Administrador" && asignacion.Materia.DocenteID != docenteId)
            {
                TempData["ErrorMessage"] = "No tienes permisos para realizar esta acción.";
                return RedirectToPage();
            }

            // Cambiar estado
            asignacion.Activa = !asignacion.Activa;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Estado de la asignación '{asignacion.Nombre}' actualizado correctamente.";
            return RedirectToPage(new { materiaId = MateriaId });
        }
    }
}