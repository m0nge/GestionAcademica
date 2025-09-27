using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Materias
{
    public class EstudiantesModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public EstudiantesModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        public Materia Materia { get; set; } = default!;
        public IList<Estudiante> EstudiantesInscritos { get; set; } = default!;
        public IList<Estudiante> EstudiantesDisponibles { get; set; } = default!;
        public bool EsAdministrador { get; set; } = false;
        public bool PuedeGestionar { get; set; } = false;

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

            // Obtener información del usuario logueado
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");
            EsAdministrador = docenteRol == "Administrador";

            // Cargar la materia con sus relaciones
            Materia = await _context.Materias
                .Include(m => m.Docente)
                .Include(m => m.Carrera)
                    .ThenInclude(c => c.Facultad)
                .FirstOrDefaultAsync(m => m.MateriaID == id);

            if (Materia == null)
            {
                return NotFound();
            }

            // Verificar permisos
            PuedeGestionar = EsAdministrador || Materia.DocenteID == docenteId;

            if (!PuedeGestionar)
            {
                TempData["ErrorMessage"] = "No tienes permisos para gestionar estudiantes de esta materia.";
                return RedirectToPage("./Index");
            }

            // Cargar estudiantes ya inscritos en esta materia
            EstudiantesInscritos = await _context.EstudianteMaterias
                .Where(em => em.MateriaID == id && em.Activa)
                .Include(em => em.Estudiante)
                    .ThenInclude(e => e.Carrera)
                .Select(em => em.Estudiante)
                .OrderBy(e => e.Apellidos)
                .ThenBy(e => e.Nombres)
                .ToListAsync();

            // Cargar estudiantes disponibles para inscribir (misma carrera, no inscritos)
            var carreraId = Materia.CarreraID;
            var estudiantesInscritosIds = EstudiantesInscritos.Select(e => e.EstudianteID).ToList();

            EstudiantesDisponibles = await _context.Estudiantes
                .Include(e => e.Carrera)
                    .ThenInclude(c => c.Facultad)
                .Where(e => e.Activo &&
                           e.CarreraID == carreraId &&
                           !estudiantesInscritosIds.Contains(e.EstudianteID))
                .OrderBy(e => e.Apellidos)
                .ThenBy(e => e.Nombres)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostInscribirAsync(int materiaId, int estudianteId)
        {
            // Verificar permisos
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");
            var esAdmin = docenteRol == "Administrador";

            var materia = await _context.Materias.FindAsync(materiaId);
            if (materia == null || (!esAdmin && materia.DocenteID != docenteId))
            {
                TempData["ErrorMessage"] = "No tienes permisos para realizar esta acción.";
                return RedirectToPage("./Index");
            }

            try
            {
                // Verificar si ya está inscrito
                var yaInscrito = await _context.EstudianteMaterias
                    .AnyAsync(em => em.MateriaID == materiaId && em.EstudianteID == estudianteId);

                if (yaInscrito)
                {
                    TempData["ErrorMessage"] = "El estudiante ya está inscrito en esta materia.";
                }
                else
                {
                    // Crear la inscripción
                    var inscripcion = new EstudianteMateria
                    {
                        EstudianteID = estudianteId,
                        MateriaID = materiaId,
                        FechaInscripcion = DateTime.Now,
                        Activa = true
                    };

                    _context.EstudianteMaterias.Add(inscripcion);
                    await _context.SaveChangesAsync();

                    var estudiante = await _context.Estudiantes.FindAsync(estudianteId);
                    TempData["SuccessMessage"] = $"Estudiante {estudiante?.Nombres} {estudiante?.Apellidos} inscrito exitosamente.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error al inscribir el estudiante. Intente nuevamente.";
            }

            return RedirectToPage("./Estudiantes", new { id = materiaId });
        }

        public async Task<IActionResult> OnPostRemoverAsync(int materiaId, int estudianteId)
        {
            // Verificar permisos
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            var docenteId = int.Parse(HttpContext.Session.GetString("DocenteID") ?? "0");
            var esAdmin = docenteRol == "Administrador";

            var materia = await _context.Materias.FindAsync(materiaId);
            if (materia == null || (!esAdmin && materia.DocenteID != docenteId))
            {
                TempData["ErrorMessage"] = "No tienes permisos para realizar esta acción.";
                return RedirectToPage("./Index");
            }

            try
            {
                var inscripcion = await _context.EstudianteMaterias
                    .FirstOrDefaultAsync(em => em.MateriaID == materiaId && em.EstudianteID == estudianteId);

                if (inscripcion != null)
                {
                    // En lugar de eliminar, desactivamos la inscripción
                    inscripcion.Activa = false;
                    await _context.SaveChangesAsync();

                    var estudiante = await _context.Estudiantes.FindAsync(estudianteId);
                    TempData["SuccessMessage"] = $"Estudiante {estudiante?.Nombres} {estudiante?.Apellidos} removido de la materia.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se encontró la inscripción del estudiante.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error al remover el estudiante. Intente nuevamente.";
            }

            return RedirectToPage("./Estudiantes", new { id = materiaId });
        }
    }
}