using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Pages.Estudiantes
{
    public class CreateModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public CreateModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Estudiante Estudiante { get; set; } = default!;

        // 🆕 NUEVAS PROPIEDADES PARA FACULTAD Y CARRERA
        public SelectList FacultadSelectList { get; set; } = default!;
        public SelectList CarreraSelectList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            // 🔒 VERIFICAR SI ES ADMIN
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            if (docenteRol != "Administrador")
            {
                TempData["ErrorMessage"] = "Solo los administradores pueden crear estudiantes.";
                return RedirectToPage("./Index");
            }

            // ✅ Cargar listas para dropdowns
            await CargarListasAsync();

            // Inicializar estudiante con valores predeterminados
            Estudiante = new Estudiante
            {
                Activo = true,
                FechaIngreso = DateTime.Now,
                CarreraID = null // Asegurar que esté vacío inicialmente
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 🔒 VERIFICAR PERMISOS EN POST
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            if (docenteRol != "Administrador")
            {
                TempData["ErrorMessage"] = "Solo los administradores pueden crear estudiantes.";
                return RedirectToPage("./Index");
            }

            // 🛡️ VALIDAR QUE SE HAYA SELECCIONADO UNA CARRERA
            if (!Estudiante.CarreraID.HasValue)
            {
                ModelState.AddModelError("Estudiante.CarreraID", "Debe seleccionar una carrera.");
                await CargarListasAsync();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                // Recargar listas si hay errores
                await CargarListasAsync();
                return Page();
            }

            try
            {
                // 📅 Establecer fecha de ingreso
                Estudiante.FechaIngreso = DateTime.Now;

                // 💾 Guardar en base de datos
                _context.Estudiantes.Add(Estudiante);
                await _context.SaveChangesAsync();

                // ✅ Mensaje de éxito
                TempData["SuccessMessage"] = $"Estudiante {Estudiante.Nombres} {Estudiante.Apellidos} creado exitosamente.";

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // ❌ Manejo de errores
                ModelState.AddModelError("", "Error al guardar el estudiante. Por favor, intente nuevamente.");

                await CargarListasAsync();
                return Page();
            }
        }

        // 🌐 ENDPOINT PARA CARRERAS DEPENDIENTES DE FACULTAD (AJAX)
        public async Task<IActionResult> OnGetCarrerasPorFacultadAsync(int facultadId)
        {
            try
            {
                var carreras = await _context.Carreras
                    .Where(c => c.FacultadID == facultadId && c.Activa)
                    .OrderBy(c => c.Nombre)
                    .Select(c => new {
                        value = c.CarreraID,
                        text = c.Nombre
                    })
                    .ToListAsync();

                return new JsonResult(carreras);
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                Console.WriteLine($"Error en OnGetCarrerasPorFacultadAsync: {ex.Message}");
                return new JsonResult(new List<object>());
            }
        }

        // 📋 CARGAR LISTAS PARA DROPDOWNS
        private async Task CargarListasAsync()
        {
            try
            {
                // Cargar facultades activas
                var facultades = await _context.Facultades
                    .Where(f => f.Activa)
                    .OrderBy(f => f.Nombre)
                    .ToListAsync();

                FacultadSelectList = new SelectList(facultades, "FacultadID", "Nombre");

                // Cargar todas las carreras para el dropdown inicial
                var carreras = await _context.Carreras
                    .Include(c => c.Facultad)
                    .Where(c => c.Activa && c.Facultad.Activa)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();

                CarreraSelectList = new SelectList(carreras, "CarreraID", "Nombre");
            }
            catch (Exception)
            {
                // Si hay error cargando listas, crear listas vacías
                FacultadSelectList = new SelectList(new List<Facultad>(), "FacultadID", "Nombre");
                CarreraSelectList = new SelectList(new List<Carrera>(), "CarreraID", "Nombre");

                ModelState.AddModelError("", "Error cargando las listas de facultades y carreras.");
            }
        }
    }
}