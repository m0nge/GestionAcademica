using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Estudiantes
{
    public class IndexModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public IndexModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        public IList<Estudiante> Estudiante { get; set; } = default!;

        // 🆕 NUEVA PROPIEDAD PARA VERIFICAR SI ES ADMIN
        public bool EsAdministrador { get; set; } = false;

        public async Task<IActionResult> OnGetAsync()
        {
            // 🔒 VERIFICAR SESIÓN
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            // 👤 VERIFICAR SI ES ADMINISTRADOR
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            EsAdministrador = docenteRol == "Administrador";

            // ❌ SOLO ADMINS PUEDEN VER LA LISTA COMPLETA
            if (!EsAdministrador)
            {
                TempData["ErrorMessage"] = "No tienes permisos para ver la lista completa de estudiantes. Los docentes solo pueden gestionar estudiantes en sus materias.";
                return RedirectToPage("/"); // Redirigir al dashboard
            }

            // ✅ Cargar estudiantes si es admin
            Estudiante = await _context.Estudiantes
                .Include(e => e.Carrera)
                    .ThenInclude(c => c.Facultad)
                .ToListAsync();

            return Page();
        }
    }
}