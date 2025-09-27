using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Materias
{
    public class EditModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public EditModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Materia Materia { get; set; } = default!;

        public SelectList DocentesDisponibles { get; set; } = default!;
        public SelectList CarrerasDisponibles { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            if (id == null || _context.Materias == null)
            {
                return NotFound();
            }

            // Cargar la materia con todas las relaciones necesarias
            var materia = await _context.Materias
                .Include(m => m.Carrera)
                    .ThenInclude(c => c.Facultad)
                .Include(m => m.Docente)
                    .ThenInclude(d => d.Facultad)
                .FirstOrDefaultAsync(m => m.MateriaID == id);

            if (materia == null)
            {
                return NotFound();
            }

            Materia = materia;

            // Obtener la facultad de la materia (a través de la carrera)
            var facultadId = Materia.Carrera?.FacultadID;

            if (facultadId.HasValue)
            {
                // Cargar solo carreras de la misma facultad
                var carreras = await _context.Carreras
                    .Where(c => c.FacultadID == facultadId.Value && c.Activa)
                    .OrderBy(c => c.Nombre)
                    .Select(c => new { c.CarreraID, c.Nombre })
                    .ToListAsync();

                CarrerasDisponibles = new SelectList(carreras, "CarreraID", "Nombre", Materia.CarreraID);

                // Cargar solo docentes de la misma facultad
                var docentes = await _context.Docentes
                    .Where(d => d.FacultadID == facultadId.Value && d.Activo)
                    .OrderBy(d => d.Apellidos)
                    .ThenBy(d => d.Nombres)
                    .Select(d => new {
                        d.DocenteID,
                        NombreCompleto = d.Apellidos + ", " + d.Nombres
                    })
                    .ToListAsync();

                DocentesDisponibles = new SelectList(docentes, "DocenteID", "NombreCompleto", Materia.DocenteID);
            }
            else
            {
                // Si no hay facultad asignada, mostrar todas las carreras y docentes
                var carreras = await _context.Carreras
                    .Where(c => c.Activa)
                    .OrderBy(c => c.Nombre)
                    .Select(c => new { c.CarreraID, c.Nombre })
                    .ToListAsync();

                CarrerasDisponibles = new SelectList(carreras, "CarreraID", "Nombre", Materia.CarreraID);

                var docentes = await _context.Docentes
                    .Where(d => d.Activo)
                    .OrderBy(d => d.Apellidos)
                    .ThenBy(d => d.Nombres)
                    .Select(d => new {
                        d.DocenteID,
                        NombreCompleto = d.Apellidos + ", " + d.Nombres
                    })
                    .ToListAsync();

                DocentesDisponibles = new SelectList(docentes, "DocenteID", "NombreCompleto", Materia.DocenteID);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                // Recargar las listas en caso de error
                await CargarListasAsync();
                return Page();
            }

            // Verificar que la materia existe
            var materiaExistente = await _context.Materias
                .Include(m => m.Carrera)
                    .ThenInclude(c => c.Facultad)
                .FirstOrDefaultAsync(m => m.MateriaID == Materia.MateriaID);

            if (materiaExistente == null)
            {
                return NotFound();
            }

            // Verificar que el código no esté duplicado (excepto para esta materia)
            var codigoDuplicado = await _context.Materias
                .AnyAsync(m => m.Codigo == Materia.Codigo && m.MateriaID != Materia.MateriaID);

            if (codigoDuplicado)
            {
                ModelState.AddModelError("Materia.Codigo", "Ya existe una materia con este código.");
                await CargarListasAsync();
                return Page();
            }

            // Verificar que la carrera y el docente pertenezcan a la misma facultad
            var carreraSeleccionada = await _context.Carreras
                .Include(c => c.Facultad)
                .FirstOrDefaultAsync(c => c.CarreraID == Materia.CarreraID);

            var docenteSeleccionado = await _context.Docentes
                .Include(d => d.Facultad)
                .FirstOrDefaultAsync(d => d.DocenteID == Materia.DocenteID);

            if (carreraSeleccionada?.FacultadID != docenteSeleccionado?.FacultadID)
            {
                ModelState.AddModelError("", "La carrera y el docente deben pertenecer a la misma facultad.");
                await CargarListasAsync();
                return Page();
            }

            try
            {
                // Actualizar los campos
                materiaExistente.Codigo = Materia.Codigo;
                materiaExistente.Nombre = Materia.Nombre;
                materiaExistente.Creditos = Materia.Creditos;
                materiaExistente.Periodo = Materia.Periodo;
                materiaExistente.CarreraID = Materia.CarreraID;
                materiaExistente.DocenteID = Materia.DocenteID;
                materiaExistente.Activa = Materia.Activa;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Materia '{Materia.Nombre}' actualizada correctamente.";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MateriaExists(Materia.MateriaID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar la materia: " + ex.Message);
                await CargarListasAsync();
                return Page();
            }
        }

        private async Task CargarListasAsync()
        {
            // Obtener la facultad de la materia actual
            var materia = await _context.Materias
                .Include(m => m.Carrera)
                    .ThenInclude(c => c.Facultad)
                .FirstOrDefaultAsync(m => m.MateriaID == Materia.MateriaID);

            var facultadId = materia?.Carrera?.FacultadID;

            if (facultadId.HasValue)
            {
                // Cargar solo carreras de la misma facultad
                var carreras = await _context.Carreras
                    .Where(c => c.FacultadID == facultadId.Value && c.Activa)
                    .OrderBy(c => c.Nombre)
                    .Select(c => new { c.CarreraID, c.Nombre })
                    .ToListAsync();

                CarrerasDisponibles = new SelectList(carreras, "CarreraID", "Nombre", Materia.CarreraID);

                // Cargar solo docentes de la misma facultad
                var docentes = await _context.Docentes
                    .Where(d => d.FacultadID == facultadId.Value && d.Activo)
                    .OrderBy(d => d.Apellidos)
                    .ThenBy(d => d.Nombres)
                    .Select(d => new {
                        d.DocenteID,
                        NombreCompleto = d.Apellidos + ", " + d.Nombres
                    })
                    .ToListAsync();

                DocentesDisponibles = new SelectList(docentes, "DocenteID", "NombreCompleto", Materia.DocenteID);
            }
            else
            {
                // Fallback: cargar todas las carreras y docentes
                var carreras = await _context.Carreras
                    .Where(c => c.Activa)
                    .OrderBy(c => c.Nombre)
                    .Select(c => new { c.CarreraID, c.Nombre })
                    .ToListAsync();

                CarrerasDisponibles = new SelectList(carreras, "CarreraID", "Nombre", Materia.CarreraID);

                var docentes = await _context.Docentes
                    .Where(d => d.Activo)
                    .OrderBy(d => d.Apellidos)
                    .ThenBy(d => d.Nombres)
                    .Select(d => new {
                        d.DocenteID,
                        NombreCompleto = d.Apellidos + ", " + d.Nombres
                    })
                    .ToListAsync();

                DocentesDisponibles = new SelectList(docentes, "DocenteID", "NombreCompleto", Materia.DocenteID);
            }
        }

        private bool MateriaExists(int id)
        {
            return (_context.Materias?.Any(e => e.MateriaID == id)).GetValueOrDefault();
        }

        // Método para obtener docentes por facultad (AJAX)
        public async Task<JsonResult> OnGetDocentesPorFacultadAsync(int facultadId)
        {
            var docentes = await _context.Docentes
                .Where(d => d.FacultadID == facultadId && d.Activo)
                .OrderBy(d => d.Apellidos)
                .ThenBy(d => d.Nombres)
                .Select(d => new {
                    Value = d.DocenteID,
                    Text = d.Apellidos + ", " + d.Nombres
                })
                .ToListAsync();

            return new JsonResult(docentes);
        }

        // Método para obtener carreras por facultad (AJAX)
        public async Task<JsonResult> OnGetCarrerasPorFacultadAsync(int facultadId)
        {
            var carreras = await _context.Carreras
                .Where(c => c.FacultadID == facultadId && c.Activa)
                .OrderBy(c => c.Nombre)
                .Select(c => new {
                    Value = c.CarreraID,
                    Text = c.Nombre
                })
                .ToListAsync();

            return new JsonResult(carreras);
        }
    }
}