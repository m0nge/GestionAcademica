using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Asignaciones
{
    public class CreateModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public CreateModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Asignacion Asignacion { get; set; } = default!;

        public Materia Materia { get; set; } = default!;
        public SelectList TiposAsignacion { get; set; } = default!;
        public decimal PorcentajeDisponible { get; set; }

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
                .Include(m => m.Asignaciones)
                .FirstOrDefaultAsync(m => m.MateriaID == MateriaId);

            if (Materia == null)
            {
                TempData["ErrorMessage"] = "Materia no encontrada.";
                return RedirectToPage("/Materias/Index");
            }

            // Verificar permisos: Solo el docente dueño puede crear asignaciones
            if (docenteRol != "Administrador" && Materia.DocenteID != docenteId)
            {
                TempData["ErrorMessage"] = "No tienes permisos para crear asignaciones en esta materia.";
                return RedirectToPage("/Materias/Index");
            }

            // Calcular porcentaje disponible
            var porcentajeUsado = await _context.Asignaciones
                .Where(a => a.MateriaID == MateriaId)
                .SumAsync(a => a.Porcentaje);

            PorcentajeDisponible = 100 - porcentajeUsado;

            // Cargar tipos de asignación
            TiposAsignacion = new SelectList(
                await _context.TiposAsignacion.ToListAsync(),
                "TipoAsignacionID",
                "Nombre"
            );

            // Inicializar modelo
            Asignacion = new Asignacion
            {
                MateriaID = MateriaId,
                FechaCreacion = DateTime.Now,
                FechaVencimiento = DateTime.Now.AddDays(7), // 1 semana por defecto
                NotaMaxima = 10,
                Activa = true
            };

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

            // Verificar que la materia existe y permisos
            var materia = await _context.Materias.FindAsync(Asignacion.MateriaID);
            if (materia == null)
            {
                TempData["ErrorMessage"] = "Materia no encontrada.";
                return RedirectToPage("/Materias/Index");
            }

            if (docenteRol != "Administrador" && materia.DocenteID != docenteId)
            {
                TempData["ErrorMessage"] = "No tienes permisos para crear asignaciones en esta materia.";
                return RedirectToPage("/Materias/Index");
            }

            // Validar porcentajes
            var porcentajeUsado = await _context.Asignaciones
                .Where(a => a.MateriaID == Asignacion.MateriaID)
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
                await OnGetAsync();
                return Page();
            }

            try
            {
                _context.Asignaciones.Add(Asignacion);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Asignación '{Asignacion.Nombre}' creada exitosamente.";
                return RedirectToPage("./Index", new { materiaId = Asignacion.MateriaID });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear la asignación. Intente nuevamente.";
                await OnGetAsync();
                return Page();
            }
        }
    }
}