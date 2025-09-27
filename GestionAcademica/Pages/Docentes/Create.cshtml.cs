using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;
using System.ComponentModel.DataAnnotations;

namespace GestionAcademica.Pages.Docentes
{
    public class CreateModel : PageModel
    {
        private readonly GestionAcademicaContext _context;

        public CreateModel(GestionAcademicaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DocenteCreateModel DocenteInput { get; set; } = new();

        public SelectList? FacultadesList { get; set; }

        public async Task<IActionResult> OnGetAsync()
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
                TempData["ErrorMessage"] = "No tienes permisos para crear docentes.";
                return RedirectToPage("/Home/Index");
            }

            // Cargar lista de facultades
            await CargarFacultades();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verificar permisos
            if (HttpContext.Session.GetString("DocenteRol") != "Administrador")
            {
                TempData["ErrorMessage"] = "No tienes permisos para crear docentes.";
                return RedirectToPage("/Home/Index");
            }

            if (!ModelState.IsValid)
            {
                await CargarFacultades();
                return Page();
            }

            try
            {
                // Verificar si el email ya existe
                var emailExiste = await _context.Docentes
                    .AnyAsync(d => d.Email == DocenteInput.Email);

                if (emailExiste)
                {
                    ModelState.AddModelError("DocenteInput.Email", "Ya existe un docente con este email.");
                    await CargarFacultades();
                    return Page();
                }

                // Crear el nuevo docente
                var nuevoDocente = new Docente
                {
                    Nombres = DocenteInput.Nombres,
                    Apellidos = DocenteInput.Apellidos,
                    Email = DocenteInput.Email,
                    Telefono = DocenteInput.Telefono,
                    Contraseña = DocenteInput.Contraseña, // En producción deberías hashear la contraseña
                    Rol = "Docente", // Los admins SOLO pueden crear docentes, no otros admins
                    FacultadID = DocenteInput.FacultadID,
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };

                _context.Docentes.Add(nuevoDocente);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Docente {nuevoDocente.NombreCompleto} creado exitosamente.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear el docente. Intente nuevamente.");
                await CargarFacultades();
                return Page();
            }
        }

        private async Task CargarFacultades()
        {
            var facultades = await _context.Facultades
                .Where(f => f.Activa && f.Codigo != "ADM") // Ocultar facultad de admin
                .OrderBy(f => f.Nombre)
                .ToListAsync();

            FacultadesList = new SelectList(facultades, "FacultadID", "Nombre");
        }

        // Validación AJAX para email
        public async Task<IActionResult> OnGetValidarEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return new JsonResult(new { exists = false });

            var exists = await _context.Docentes.AnyAsync(d => d.Email == email);
            return new JsonResult(new { exists });
        }
    }

    // Modelo específico para la creación (sin propiedades calculadas)
    public class DocenteCreateModel
    {
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

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contraseña { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una facultad")]
        [Display(Name = "Facultad")]
        public int FacultadID { get; set; }
    }
}