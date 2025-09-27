using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;
using System.ComponentModel.DataAnnotations;

namespace GestionAcademica.Pages.Docentes
{
    public class EditModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public EditModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DocenteEditModel DocenteInput { get; set; } = new();

        public SelectList? FacultadesList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Verificar sesión
            if (HttpContext.Session.GetString("DocenteLogueado") == null)
            {
                return RedirectToPage("/Login");
            }

            // Verificar que sea administrador
            var docenteRol = HttpContext.Session.GetString("DocenteRol") ?? "Docente";
            if (docenteRol != "Administrador")
            {
                TempData["ErrorMessage"] = "No tienes permisos para editar docentes.";
                return RedirectToPage("/Home/Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var docente = await _context.Docentes
                .Include(d => d.Facultad)
                .FirstOrDefaultAsync(m => m.DocenteID == id);

            if (docente == null)
            {
                return NotFound();
            }

            // Mapear el docente al modelo de edición
            DocenteInput = new DocenteEditModel
            {
                DocenteID = docente.DocenteID,
                Nombres = docente.Nombres,
                Apellidos = docente.Apellidos,
                Email = docente.Email,
                Telefono = docente.Telefono,
                FacultadID = docente.FacultadID,
                Rol = docente.Rol,
                Activo = docente.Activo,
                EmailOriginal = docente.Email // Para validación
            };

            await CargarFacultades();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verificar permisos
            if (HttpContext.Session.GetString("DocenteRol") != "Administrador")
            {
                TempData["ErrorMessage"] = "No tienes permisos para editar docentes.";
                return RedirectToPage("/Home/Index");
            }

            if (!ModelState.IsValid)
            {
                await CargarFacultades();
                return Page();
            }

            try
            {
                // Verificar si el email ya existe (excepto el actual)
                if (DocenteInput.Email != DocenteInput.EmailOriginal)
                {
                    var emailExiste = await _context.Docentes
                        .AnyAsync(d => d.Email == DocenteInput.Email && d.DocenteID != DocenteInput.DocenteID);

                    if (emailExiste)
                    {
                        ModelState.AddModelError("DocenteInput.Email", "Ya existe otro docente con este email.");
                        await CargarFacultades();
                        return Page();
                    }
                }

                // Buscar el docente a actualizar
                var docenteExistente = await _context.Docentes.FindAsync(DocenteInput.DocenteID);
                if (docenteExistente == null)
                {
                    return NotFound();
                }

                // Actualizar los campos
                docenteExistente.Nombres = DocenteInput.Nombres;
                docenteExistente.Apellidos = DocenteInput.Apellidos;
                docenteExistente.Email = DocenteInput.Email;
                docenteExistente.Telefono = DocenteInput.Telefono;
                docenteExistente.FacultadID = DocenteInput.FacultadID;
                docenteExistente.Rol = DocenteInput.Rol;
                docenteExistente.Activo = DocenteInput.Activo;

                // Solo actualizar contraseña si se proporcionó una nueva
                if (!string.IsNullOrEmpty(DocenteInput.NuevaContraseña))
                {
                    docenteExistente.Contraseña = DocenteInput.NuevaContraseña;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Docente {docenteExistente.NombreCompleto} actualizado exitosamente.";
                return RedirectToPage("./Details", new { id = DocenteInput.DocenteID });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocenteExists(DocenteInput.DocenteID))
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
                ModelState.AddModelError("", "Error al actualizar el docente. Intente nuevamente.");
                await CargarFacultades();
                return Page();
            }
        }

        // Validación AJAX para email
        public async Task<IActionResult> OnGetValidarEmailAsync(string email, int docenteId)
        {
            if (string.IsNullOrEmpty(email))
                return new JsonResult(new { exists = false });

            var exists = await _context.Docentes
                .AnyAsync(d => d.Email == email && d.DocenteID != docenteId);

            return new JsonResult(new { exists });
        }

        private async Task CargarFacultades()
        {
            var facultades = await _context.Facultades
                .Where(f => f.Activa && f.Codigo != "ADM") // Ocultar facultad de admin
                .OrderBy(f => f.Nombre)
                .ToListAsync();

            FacultadesList = new SelectList(facultades, "FacultadID", "Nombre");
        }

        private bool DocenteExists(int id)
        {
            return _context.Docentes.Any(e => e.DocenteID == id);
        }
    }

    // Modelo específico para la edición
    public class DocenteEditModel
    {
        public int DocenteID { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden exceder 100 caracteres")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(255, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string? NuevaContraseña { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una facultad")]
        [Display(Name = "Facultad")]
        public int? FacultadID { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = "Docente";

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Campo oculto para validación de email
        public string EmailOriginal { get; set; } = string.Empty;
    }
}