using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GestionAcademica.Models
{
    public class Estudiante
    {
        [Key]
        public int EstudianteID { get; set; }
        [Required(ErrorMessage = "El carnet es obligatorio")]
        [StringLength(20, ErrorMessage = "El carnet no puede exceder 20 caracteres")]
        [Display(Name = "Carnet")]
        public string Carnet { get; set; } = string.Empty;
        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden exceder 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "Los nombres solo pueden contener letras y espacios")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; } = string.Empty;
        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "Los apellidos solo pueden contener letras y espacios")]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = string.Empty;
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        [Display(Name = "Email")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "El formato debe ser: 0000-0000")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [YearRange(1900, 2009, ErrorMessage = "El año debe estar entre 1900 y 2009")]
        public DateTime? FechaNacimiento { get; set; }
        [Display(Name = "Fecha de Ingreso")]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;
        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
        // Agregar estas líneas en tu clase Estudiante:

        [Display(Name = "Carrera")]
        public int? CarreraID { get; set; }

        // Propiedad de navegación
        [ForeignKey("CarreraID")]
        public virtual Carrera? Carrera { get; set; }
        // Propiedad calculada para mostrar nombre completo
        [NotMapped]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Nombres} {Apellidos}";
        // Propiedad calculada para mostrar edad
        [NotMapped]
        [Display(Name = "Edad")]

        public int? Edad
        {
            get
            {
                if (FechaNacimiento.HasValue)
                {
                    var today = DateTime.Today;
                    var age = today.Year - FechaNacimiento.Value.Year;
                    if (FechaNacimiento.Value.Date > today.AddYears(-age))
                        age--;
                    return age;
                }
                return null;
            }
        }
    }
}