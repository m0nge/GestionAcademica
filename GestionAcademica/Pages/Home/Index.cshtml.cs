using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public IndexModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        // Propiedades para la vista
        public string DocenteNombre { get; set; } = "";
        public string DocenteRol { get; set; } = "";
        public bool EsAdministrador { get; set; } = false;
        public int TotalEstudiantes { get; set; } = 0;
        public int TotalDocentes { get; set; } = 0;

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            // Obtener información del usuario logueado
            DocenteNombre = HttpContext.Session.GetString("DocenteNombre") ?? "Usuario";
            DocenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            EsAdministrador = DocenteRol == "Administrador";

            // Obtener estadísticas de la base de datos
            try
            {
                TotalEstudiantes = await _context.Estudiantes.CountAsync();

                if (EsAdministrador)
                {
                    TotalDocentes = await _context.Docentes.CountAsync(d => d.Activo);
                }
            }
            catch
            {
                TotalEstudiantes = 0;
                TotalDocentes = 0;
            }

            return Page();
        }
    }
}