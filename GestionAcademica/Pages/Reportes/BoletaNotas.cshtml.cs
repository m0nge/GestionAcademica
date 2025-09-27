using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Reportes
{
    public class BoletaNotasModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public BoletaNotasModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        public Estudiante Estudiante { get; set; } = default!;
        public IList<MateriaConCalificaciones> MateriasConCalificaciones { get; set; } = default!;
        public EstadisticasEstudiante Estadisticas { get; set; } = default!;
        public bool EsPDF { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public int EstudianteId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Formato { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            // Verificar formato
            EsPDF = !string.IsNullOrEmpty(Formato) && Formato.ToLower() == "pdf";

            // Obtener información del usuario logueado
            var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            var esAdministrador = docenteRol == "Administrador";

            // Cargar estudiante
            Estudiante = await _context.Estudiantes
                .Include(e => e.Carrera)
                    .ThenInclude(c => c.Facultad)
                .FirstOrDefaultAsync(e => e.EstudianteID == EstudianteId);

            if (Estudiante == null)
            {
                TempData["ErrorMessage"] = "Estudiante no encontrado.";
                return RedirectToPage("./Index");
            }

            // Verificar permisos
            if (!esAdministrador)
            {
                // Verificar que el docente tenga acceso a este estudiante
                var tieneAcceso = await _context.EstudianteMaterias
    .Include(em => em.Materia)
    .AnyAsync(em => em.EstudianteID == EstudianteId &&
                    em.Materia.DocenteID == docenteId &&
                    em.Activa == true);

                if (!tieneAcceso)
                {
                    TempData["ErrorMessage"] = "No tienes permisos para ver la boleta de este estudiante.";
                    return RedirectToPage("./Index");
                }
            }

            // Cargar materias con calificaciones
            await CargarMateriasConCalificaciones(docenteId, esAdministrador);

            // Calcular estadísticas
            CalcularEstadisticas();

            return Page();
        }

        private async Task CargarMateriasConCalificaciones(int docenteId, bool esAdministrador)
        {
            // Query base para materias del estudiante
            var queryMaterias = _context.EstudianteMaterias
    .Include(em => em.Materia)
        .ThenInclude(m => m.Docente)
    .Include(em => em.Materia)
        .ThenInclude(m => m.Carrera)
    .Where(em => em.EstudianteID == EstudianteId && em.Activa == true);

            // Si no es administrador, filtrar solo materias del docente
            if (!esAdministrador)
            {
                queryMaterias = queryMaterias.Where(em => em.Materia.DocenteID == docenteId);
            }

            var estudianteMaterias = await queryMaterias.ToListAsync();

            MateriasConCalificaciones = new List<MateriaConCalificaciones>();

            foreach (var estudianteMateria in estudianteMaterias)
            {
                var materia = estudianteMateria.Materia;

                // Cargar asignaciones de la materia
                var asignaciones = await _context.Asignaciones
                    .Include(a => a.TipoAsignacion)
                    .Where(a => a.MateriaID == materia.MateriaID && a.Activa)
                    .OrderBy(a => a.FechaCreacion)
                    .ToListAsync();

                var asignacionesConCalificaciones = new List<AsignacionConCalificacion>();

                foreach (var asignacion in asignaciones)
                {
                    // Buscar calificación del estudiante para esta asignación
                    var calificacion = await _context.Calificaciones
                        .FirstOrDefaultAsync(c => c.AsignacionID == asignacion.AsignacionID &&
                                                 c.EstudianteID == EstudianteId);

                    asignacionesConCalificaciones.Add(new AsignacionConCalificacion
                    {
                        Asignacion = asignacion,
                        Calificacion = calificacion,
                        TieneCalificacion = calificacion != null
                    });
                }

                // Calcular nota final de la materia
                var notaFinal = CalcularNotaFinalMateria(asignacionesConCalificaciones);
                var porcentajeCompleto = asignacionesConCalificaciones.Sum(ac => ac.TieneCalificacion ? ac.Asignacion.Porcentaje : 0);

                MateriasConCalificaciones.Add(new MateriaConCalificaciones
                {
                    Materia = materia,
                    AsignacionesConCalificaciones = asignacionesConCalificaciones,
                    NotaFinal = notaFinal,
                    PorcentajeCompleto = porcentajeCompleto,
                    EstadoMateria = ObtenerEstadoMateria(notaFinal, porcentajeCompleto)
                });
            }

            // Ordenar por nombre de materia
            MateriasConCalificaciones = MateriasConCalificaciones.OrderBy(m => m.Materia.Nombre).ToList();
        }

        private decimal CalcularNotaFinalMateria(List<AsignacionConCalificacion> asignaciones)
        {
            decimal notaFinal = 0;
            decimal porcentajeTotal = 0;

            foreach (var asignacionConCalif in asignaciones)
            {
                if (asignacionConCalif.TieneCalificacion)
                {
                    var porcentajeAsignacion = asignacionConCalif.Asignacion.Porcentaje / 100;
                    var notaNormalizada = asignacionConCalif.Calificacion!.Nota / asignacionConCalif.Asignacion.NotaMaxima * 10; // Normalizar a escala de 10

                    notaFinal += notaNormalizada * porcentajeAsignacion;
                    porcentajeTotal += porcentajeAsignacion;
                }
            }

            return porcentajeTotal > 0 ? notaFinal : 0;
        }

        private string ObtenerEstadoMateria(decimal notaFinal, decimal porcentajeCompleto)
        {
            if (porcentajeCompleto < 100)
                return "En Curso";

            if (notaFinal >= 6)
                return "Aprobada";
            else if (notaFinal >= 4)
                return "En Recuperación";
            else
                return "Reprobada";
        }

        private void CalcularEstadisticas()
        {
            var materiasConNota = MateriasConCalificaciones.Where(m => m.NotaFinal > 0).ToList();

            Estadisticas = new EstadisticasEstudiante
            {
                TotalMaterias = MateriasConCalificaciones.Count,
                MateriasAprobadas = MateriasConCalificaciones.Count(m => m.EstadoMateria == "Aprobada"),
                MateriasReprobadas = MateriasConCalificaciones.Count(m => m.EstadoMateria == "Reprobada"),
                MateriasEnCurso = MateriasConCalificaciones.Count(m => m.EstadoMateria == "En Curso"),
                PromedioGeneral = materiasConNota.Any() ? materiasConNota.Average(m => m.NotaFinal) : 0,
                NotaMasAlta = materiasConNota.Any() ? materiasConNota.Max(m => m.NotaFinal) : 0,
                NotaMasBaja = materiasConNota.Any() ? materiasConNota.Min(m => m.NotaFinal) : 0,
                TotalAsignaciones = MateriasConCalificaciones.Sum(m => m.AsignacionesConCalificaciones.Count),
                AsignacionesCalificadas = MateriasConCalificaciones.Sum(m => m.AsignacionesConCalificaciones.Count(a => a.TieneCalificacion))
            };
        }
    }

    public class MateriaConCalificaciones
    {
        public Materia Materia { get; set; } = default!;
        public List<AsignacionConCalificacion> AsignacionesConCalificaciones { get; set; } = new();
        public decimal NotaFinal { get; set; }
        public decimal PorcentajeCompleto { get; set; }
        public string EstadoMateria { get; set; } = "";
    }

    public class AsignacionConCalificacion
    {
        public Asignacion Asignacion { get; set; } = default!;
        public Calificacion? Calificacion { get; set; }
        public bool TieneCalificacion { get; set; }
    }

    public class EstadisticasEstudiante
    {
        public int TotalMaterias { get; set; }
        public int MateriasAprobadas { get; set; }
        public int MateriasReprobadas { get; set; }
        public int MateriasEnCurso { get; set; }
        public decimal PromedioGeneral { get; set; }
        public decimal NotaMasAlta { get; set; }
        public decimal NotaMasBaja { get; set; }
        public int TotalAsignaciones { get; set; }
        public int AsignacionesCalificadas { get; set; }
    }
}