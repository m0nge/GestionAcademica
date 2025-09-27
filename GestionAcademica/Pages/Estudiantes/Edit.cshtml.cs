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
    public class EditModel : PageModel
    {
        private readonly GestionAcademica.Models.GestionAcademicaContext _context;

        public EditModel(GestionAcademica.Models.GestionAcademicaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Estudiante Estudiante { get; set; } = default!;

        // 🆕 PROPIEDADES PARA FACULTAD Y CARRERA
        public SelectList FacultadSelectList { get; set; } = default!;
        public SelectList CarreraSelectList { get; set; } = default!;
        public int? FacultadSeleccionada { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // 📋 CARGAR ESTUDIANTE CON CARRERA Y FACULTAD
            var estudiante = await _context.Estudiantes
                .Include(e => e.Carrera)
                    .ThenInclude(c => c.Facultad)
                .FirstOrDefaultAsync(m => m.EstudianteID == id);

            if (estudiante == null)
            {
                return NotFound();
            }

            Estudiante = estudiante;

            // ✅ Establecer facultad seleccionada si el estudiante tiene carrera
            if (Estudiante.Carrera?.FacultadID != null)
            {
                FacultadSeleccionada = Estudiante.Carrera.FacultadID;
            }

            // ✅ Cargar listas para dropdowns
            await CargarListasAsync();

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            // 🛡️ VALIDAR QUE SE HAYA SELECCIONADO UNA CARRERA
            if (!Estudiante.CarreraID.HasValue)
            {
                ModelState.AddModelError("Estudiante.CarreraID", "Debe seleccionar una carrera.");
                await CargarListasAsync();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await CargarListasAsync();
                return Page();
            }

            _context.Attach(Estudiante).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstudianteExists(Estudiante.EstudianteID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
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

                FacultadSelectList = new SelectList(facultades, "FacultadID", "Nombre", FacultadSeleccionada);

                // Cargar carreras de la facultad seleccionada o todas si no hay selección
                if (FacultadSeleccionada.HasValue)
                {
                    var carrerasFacultad = await _context.Carreras
                        .Where(c => c.FacultadID == FacultadSeleccionada && c.Activa)
                        .OrderBy(c => c.Nombre)
                        .ToListAsync();

                    CarreraSelectList = new SelectList(carrerasFacultad, "CarreraID", "Nombre", Estudiante.CarreraID);
                }
                else
                {
                    // Cargar todas las carreras para el dropdown inicial
                    var carreras = await _context.Carreras
                        .Include(c => c.Facultad)
                        .Where(c => c.Activa && c.Facultad.Activa)
                        .OrderBy(c => c.Nombre)
                        .ToListAsync();

                    CarreraSelectList = new SelectList(carreras, "CarreraID", "Nombre", Estudiante.CarreraID);
                }
            }
            catch (Exception)
            { }
        } 
        private bool EstudianteExists(int id)
        {
            return _context.Estudiantes.Any(e => e.EstudianteID == id);
        }
    }
}