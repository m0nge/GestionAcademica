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
    public class DetailsModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public DetailsModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        public Estudiante Estudiante { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // 🔒 VERIFICAR SESIÓN
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            // 🔒 VERIFICAR PERMISOS - Solo Admin puede ver detalles completos
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            if (docenteRol != "Administrador")
            {
                TempData["ErrorMessage"] = "Solo los administradores pueden ver los detalles completos de estudiantes.";
                return RedirectToPage("./Index");
            }

            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de estudiante no proporcionado.";
                return RedirectToPage("./Index");
            }

            try
            {
                // 📋 CARGAR ESTUDIANTE CON TODAS LAS RELACIONES
                var estudiante = await _context.Estudiantes
                    .Include(e => e.Carrera)                    // ✅ Incluir Carrera
                        .ThenInclude(c => c.Facultad)           // ✅ Incluir Facultad a través de Carrera
                    .FirstOrDefaultAsync(m => m.EstudianteID == id);

                if (estudiante == null)
                {
                    TempData["ErrorMessage"] = $"No se encontró el estudiante con ID {id}.";
                    return RedirectToPage("./Index");
                }

                Estudiante = estudiante;
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar los detalles del estudiante. Por favor, intente nuevamente.";
                return RedirectToPage("./Index");
            }
        }
    }
}