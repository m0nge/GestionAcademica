using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Asignaciones
{
    public class EditModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public EditModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Asignacion Asignacion { get; set; } = default!;

        public Materia Materia { get; set; } = default!;
        public SelectList TiposAsignacion { get; set; } = default!;
        public decimal PorcentajeDisponible { get; set; }
        public decimal PorcentajeOriginal { get; set; }

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

            // Cargar la asignación
            Asignacion = await _context.Asignaciones
                .Include(a => a.Materia)
                .Include(a => a.TipoAsignacion)
                .FirstOrDefaultAsync(a => a.AsignacionID == id);

            if (Asignacion == null)
            {
                return NotFound();
            }

            // Obtener información del docente logueado
            var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";

            // Verificar permisos: Solo el docente dueño puede editar sus asignaciones
            if (docenteRol != "Administrador" && Asignacion.Materia.DocenteID != docenteId)
            {
                TempData["ErrorMessage"] = "No tienes permisos para editar esta asignación.";
                return RedirectToPage("/Materias/Index");
            }

            // Cargar materia
            Materia = Asignacion.Materia;
            MateriaId = Asignacion.MateriaID;

            // Calcular porcentaje disponible (excluyendo la asignación actual)
            var porcentajeUsado = await _context.Asignaciones
                .Where(a => a.MateriaID == Asignacion.MateriaID && a.AsignacionID != Asignacion.AsignacionID)
                .SumAsync(a => a.Porcentaje);

            PorcentajeOriginal = Asignacion.Porcentaje;
            PorcentajeDisponible = 100 - porcentajeUsado;

            // Cargar tipos de asignación
            TiposAsignacion = new SelectList(
                await _context.TiposAsignacion.ToListAsync(),
                "TipoAsignacionID",
                "Nombre",
                Asignacion.TipoAsignacionID
            );

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            // Obtener información del docente logueado
            var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";

            // Verificar que la asignación existe y permisos
            var asignacionOriginal = await _context.Asignaciones
                .Include(a => a.Materia)
                .FirstOrDefaultAsync(a => a.AsignacionID == Asignacion.AsignacionID);

            if (asignacionOriginal == null)
            {
                TempData["ErrorMessage"] = "Asignación no encontrada.";
                return RedirectToPage("/Materias/Index");
            }

            if (docenteRol != "Administrador" && asignacionOriginal.Materia.DocenteID != docenteId)
            {
                TempData["ErrorMessage"] = "No tienes permisos para editar esta asignación.";
                return RedirectToPage("/Materias/Index");
            }

            // Validar porcentajes
            var porcentajeUsado = await _context.Asignaciones
                .Where(a => a.MateriaID == Asignacion.MateriaID && a.AsignacionID != Asignacion.AsignacionID)
                .SumAsync(a => a.Porcentaje);

            if (porcentajeUsado + Asignacion.Porcentaje > 100)
            {
                ModelState.AddModelError("Asignacion.Porcentaje",
                    $"El porcentaje excede el disponible. Disponible: {100 - porcentajeUsado}%");
            }

            // Validar fechas
            if (Asignacion.FechaVencimiento <= Asignacion.FechaCreacion)
            {
                ModelState.AddModelError("Asignacion.FechaVencimiento",
                    "La fecha de vencimiento debe ser posterior a la fecha de creación.");
            }

            if (!ModelState.IsValid)
            {
                // Recargar datos para mostrar el formulario de nuevo
                await OnGetAsync(Asignacion.AsignacionID);
                return Page();
            }

            try
            {
                // Actualizar solo los campos que pueden cambiar
                asignacionOriginal.Nombre = Asignacion.Nombre;
                asignacionOriginal.Descripcion = Asignacion.Descripcion;
                asignacionOriginal.TipoAsignacionID = Asignacion.TipoAsignacionID;
                asignacionOriginal.Porcentaje = Asignacion.Porcentaje;
                asignacionOriginal.FechaVencimiento = Asignacion.FechaVencimiento;
                asignacionOriginal.NotaMaxima = Asignacion.NotaMaxima;
                asignacionOriginal.Activa = Asignacion.Activa;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Asignación '{Asignacion.Nombre}' actualizada exitosamente.";
                return RedirectToPage("./Index", new { materiaId = Asignacion.MateriaID });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar la asignación. Intente nuevamente.";
                await OnGetAsync(Asignacion.AsignacionID);
                return Page();
            }
        }
    }
}