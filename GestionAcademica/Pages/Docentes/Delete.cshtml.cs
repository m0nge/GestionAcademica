using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Docentes
{
    public class DeleteModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public DeleteModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        public Docente Docente { get; set; } = default!;
        public IList<Materia> MateriasAfectadas { get; set; } = default!;
        public bool EsAdministrador { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            // Verificar que sea administrador
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            EsAdministrador = docenteRol == "Administrador";

            if (!EsAdministrador)
            {
                TempData["ErrorMessage"] = "No tienes permisos para realizar esta acción.";
                return RedirectToPage("/Home/Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            // Cargar docente con su facultad
            var docente = await _context.Docentes
                .Include(d => d.Facultad)
                .FirstOrDefaultAsync(m => m.DocenteID == id);

            if (docente == null)
            {
                return NotFound();
            }

            Docente = docente;

            // Cargar materias que serían afectadas
            MateriasAfectadas = await _context.Materias
                .Include(m => m.Carrera)
                .Where(m => m.DocenteID == id)
                .OrderBy(m => m.Codigo)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // Verificar permisos
            if (HttpContext.Session.GetString("DocenteRol") != "Administrador")
            {
                TempData["ErrorMessage"] = "No tienes permisos para realizar esta acción.";
                return RedirectToPage("./Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var docente = await _context.Docentes.FindAsync(id);
            if (docente != null)
            {
                // En lugar de eliminar, cambiamos el estado
                docente.Activo = !docente.Activo;
                await _context.SaveChangesAsync();

                string accion = docente.Activo ? "activado" : "desactivado";
                TempData["SuccessMessage"] = $"Docente {docente.NombreCompleto} {accion} exitosamente.";
            }

            return RedirectToPage("./Index");
        }
    }
}