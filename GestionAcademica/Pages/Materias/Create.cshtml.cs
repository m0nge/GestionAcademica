using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Materias
{
    public class CreateModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public CreateModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Materia Materia { get; set; } = default!;

        public SelectList FacultadesDisponibles { get; set; } = default!;
        public SelectList CarrerasDisponibles { get; set; } = default!;
        public SelectList DocentesDisponibles { get; set; } = default!;

        [BindProperty]
        public int FacultadSeleccionada { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar sesión y permisos de administrador
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            if (docenteRol != "Administrador")
            {
                TempData["ErrorMessage"] = "No tienes permisos para crear materias.";
                return RedirectToPage("/Home/Index");
            }

            await CargarListas();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verificar permisos nuevamente
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            if (docenteRol != "Administrador")
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await CargarListas();
                return Page();
            }

            try
            {
                // Verificar que el código no exista
                var codigoExiste = await _context.Materias
                    .AnyAsync(m => m.Codigo.ToLower() == Materia.Codigo.ToLower());

                if (codigoExiste)
                {
                    ModelState.AddModelError("Materia.Codigo", "Ya existe una materia con este código.");
                    await CargarListas();
                    return Page();
                }

                // Verificar que el docente existe y está activo
                var docente = await _context.Docentes
                    .Include(d => d.Facultad)
                    .FirstOrDefaultAsync(d => d.DocenteID == Materia.DocenteID && d.Activo);

                if (docente == null)
                {
                    ModelState.AddModelError("Materia.DocenteID", "El docente seleccionado no existe o está inactivo.");
                    await CargarListas();
                    return Page();
                }

                // Verificar que la carrera pertenece a la facultad del docente
                var carrera = await _context.Carreras
                    .Include(c => c.Facultad)
                    .FirstOrDefaultAsync(c => c.CarreraID == Materia.CarreraID);

                if (carrera == null)
                {
                    ModelState.AddModelError("Materia.CarreraID", "Debe seleccionar una carrera válida.");
                    await CargarListas();
                    return Page();
                }

                if (docente.FacultadID != carrera.FacultadID)
                {
                    ModelState.AddModelError("Materia.CarreraID", $"La carrera seleccionada no pertenece a la facultad del docente ({docente.Facultad?.Nombre}).");
                    await CargarListas();
                    return Page();
                }

                // Establecer valores por defecto
                Materia.Activa = true;

                _context.Materias.Add(Materia);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Materia '{Materia.Nombre}' creada exitosamente en {carrera.Nombre} y asignada a {docente.NombreCompleto}.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear la materia. Intente nuevamente.");
                await CargarListas();
                return Page();
            }
        }

        // Método AJAX para obtener carreras por facultad
        public async Task<IActionResult> OnGetCarrerasPorFacultadAsync(int facultadId)
        {
            var carreras = await _context.Carreras
                .Where(c => c.FacultadID == facultadId && c.Activa)
                .OrderBy(c => c.Nombre)
                .Select(c => new { value = c.CarreraID, text = c.Nombre })
                .ToListAsync();

            return new JsonResult(carreras);
        }

        // Método AJAX para obtener docentes por facultad
        public async Task<IActionResult> OnGetDocentesPorFacultadAsync(int facultadId)
        {
            var docentes = await _context.Docentes
                .Where(d => d.FacultadID == facultadId && d.Activo)
                .OrderBy(d => d.Nombres)
                .Select(d => new { value = d.DocenteID, text = $"{d.Nombres} {d.Apellidos}" })
                .ToListAsync();

            return new JsonResult(docentes);
        }

        private async Task CargarListas()
        {
            // Cargar facultades (sin la de admin)
            var facultades = await _context.Facultades
                .Where(f => f.Activa && f.Codigo != "ADM") // Ocultar facultad de admin
                .OrderBy(f => f.Nombre)
                .ToListAsync();

            FacultadesDisponibles = new SelectList(facultades, "FacultadID", "Nombre");

            // Inicializar listas vacías (se llenarán con JavaScript)
            CarrerasDisponibles = new SelectList(new List<object>(), "Value", "Text");
            DocentesDisponibles = new SelectList(new List<object>(), "Value", "Text");
        }
    }
}