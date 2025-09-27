using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Calificaciones
{
    public class IndexModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public IndexModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        public Asignacion Asignacion { get; set; } = default!;
        public Materia Materia { get; set; } = default!;
        public IList<EstudianteCalificacion> EstudiantesCalificaciones { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public int AsignacionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int MateriaId { get; set; }

        [BindProperty]
        public List<CalificacionInput> Calificaciones { get; set; } = new();

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

            // Cargar la asignación con todas las relaciones
            Asignacion = await _context.Asignaciones
                .Include(a => a.Materia)
                    .ThenInclude(m => m.Docente)
                .Include(a => a.TipoAsignacion)
                .FirstOrDefaultAsync(a => a.AsignacionID == AsignacionId);

            if (Asignacion == null)
            {
                TempData["ErrorMessage"] = "Asignación no encontrada.";
                return RedirectToPage("/Materias/Index");
            }

            // Verificar permisos: Solo el docente dueño puede calificar
            if (docenteRol != "Administrador" && Asignacion.Materia.DocenteID != docenteId)
            {
                TempData["ErrorMessage"] = "No tienes permisos para calificar esta asignación.";
                return RedirectToPage("/Materias/Index");
            }

            Materia = Asignacion.Materia;

            // Obtener estudiantes inscritos en la materia
            // Obtener estudiantes inscritos en la materia desde EstudianteMaterias
            var estudiantesInscritos = await _context.EstudianteMaterias
                .Where(em => em.MateriaID == Asignacion.MateriaID && em.Activa)
                .Select(em => em.Estudiante)
                .Where(e => e.Activo)
                .OrderBy(e => e.Carnet)
                .ToListAsync();

            // Obtener calificaciones existentes para esta asignación
            var calificacionesExistentes = await _context.Calificaciones
                .Where(c => c.AsignacionID == AsignacionId)
                .ToListAsync();

            // Crear lista de estudiantes con sus calificaciones
            EstudiantesCalificaciones = estudiantesInscritos.Select(estudiante =>
            {
                var calificacionExistente = calificacionesExistentes
                    .FirstOrDefault(c => c.EstudianteID == estudiante.EstudianteID);

                return new EstudianteCalificacion
                {
                    Estudiante = estudiante,
                    Calificacion = calificacionExistente,
                    TieneCalificacion = calificacionExistente != null
                };
            }).ToList();

            // Preparar lista para el binding del formulario
            Calificaciones = EstudiantesCalificaciones.Select(ec => new CalificacionInput
            {
                EstudianteID = ec.Estudiante.EstudianteID,
                AsignacionID = AsignacionId,
                Nota = ec.Calificacion?.Nota ?? 0,
                Observaciones = ec.Calificacion?.Observaciones ?? "",
                CalificacionID = ec.Calificacion?.CalificacionID ?? 0
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verificar sesión y permisos
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";

            // Verificar que la asignación existe y permisos
            var asignacion = await _context.Asignaciones
                .Include(a => a.Materia)
                .FirstOrDefaultAsync(a => a.AsignacionID == AsignacionId);

            if (asignacion == null || (docenteRol != "Administrador" && asignacion.Materia.DocenteID != docenteId))
            {
                TempData["ErrorMessage"] = "No tienes permisos para realizar esta acción.";
                return RedirectToPage("/Materias/Index");
            }

            try
            {
                int calificacionesActualizadas = 0;
                int calificacionesCreadas = 0;

                foreach (var calificacionInput in Calificaciones)
                {
                    // Validar nota dentro del rango permitido
                    if (calificacionInput.Nota < 0 || calificacionInput.Nota > asignacion.NotaMaxima)
                    {
                        continue; // Saltar calificaciones inválidas
                    }

                    // Buscar si ya existe una calificación
                    var calificacionExistente = await _context.Calificaciones
                        .FirstOrDefaultAsync(c => c.AsignacionID == calificacionInput.AsignacionID &&
                                                 c.EstudianteID == calificacionInput.EstudianteID);

                    if (calificacionExistente != null)
                    {
                        // Actualizar calificación existente
                        calificacionExistente.Nota = calificacionInput.Nota;
                        calificacionExistente.Observaciones = calificacionInput.Observaciones;
                        calificacionExistente.FechaCalificacion = DateTime.Now;
                        calificacionesActualizadas++;
                    }
                    else if (calificacionInput.Nota > 0) // Solo crear si hay una nota mayor a 0
                    {
                        // Crear nueva calificación
                        var nuevaCalificacion = new Calificacion
                        {
                            AsignacionID = calificacionInput.AsignacionID,
                            EstudianteID = calificacionInput.EstudianteID,
                            Nota = calificacionInput.Nota,
                            Observaciones = calificacionInput.Observaciones,
                            FechaCalificacion = DateTime.Now
                        };
                        _context.Calificaciones.Add(nuevaCalificacion);
                        calificacionesCreadas++;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Calificaciones guardadas exitosamente. " +
                    $"Creadas: {calificacionesCreadas}, Actualizadas: {calificacionesActualizadas}";

                return RedirectToPage(new { asignacionId = AsignacionId, materiaId = MateriaId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al guardar las calificaciones. Intente nuevamente.";
                await OnGetAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEliminarCalificacionAsync(int calificacionId)
        {
            // Verificar sesión y permisos
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var calificacion = await _context.Calificaciones
                    .Include(c => c.Asignacion)
                        .ThenInclude(a => a.Materia)
                    .FirstOrDefaultAsync(c => c.CalificacionID == calificacionId);

                if (calificacion != null)
                {
                    var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");
                    var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";

                    // Verificar permisos
                    if (docenteRol == "Administrador" || calificacion.Asignacion.Materia.DocenteID == docenteId)
                    {
                        _context.Calificaciones.Remove(calificacion);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Calificación eliminada exitosamente.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "No tienes permisos para eliminar esta calificación.";
                    }
                }

                return RedirectToPage(new { asignacionId = AsignacionId, materiaId = MateriaId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la calificación.";
                return RedirectToPage(new { asignacionId = AsignacionId, materiaId = MateriaId });
            }
        }
    }

    public class EstudianteCalificacion
    {
        public Estudiante Estudiante { get; set; } = default!;
        public Calificacion? Calificacion { get; set; }
        public bool TieneCalificacion { get; set; }
    }

    public class CalificacionInput
    {
        public int CalificacionID { get; set; }
        public int EstudianteID { get; set; }
        public int AsignacionID { get; set; }
        public decimal Nota { get; set; }
        public string Observaciones { get; set; } = "";
    }
}