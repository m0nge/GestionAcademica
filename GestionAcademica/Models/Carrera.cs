using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionAcademica.Models
{
    public class Carrera
    {
        [Key]
        // Agregar estas líneas en tu clase Carrera:

        [Display(Name = "Facultad")]
        public int FacultadID { get; set; }

        // Propiedad de navegación hacia Facultad
        [ForeignKey("FacultadID")]
        public virtual Facultad? Facultad { get; set; }

        public int CarreraID { get; set; }

        [Required(ErrorMessage = "El nombre de la carrera es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre de la Carrera")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código de la carrera es obligatorio")]
        [StringLength(10, ErrorMessage = "El código no puede exceder 10 caracteres")]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La duración en años es obligatoria")]
        [Range(1, 10, ErrorMessage = "La duración debe estar entre 1 y 10 años")]
        [Display(Name = "Duración (Años)")]
        public int DuracionAnios { get; set; } = 4;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        // Navegación hacia estudiantes
        public virtual ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();

        // Navegación hacia materias (relación muchos a muchos)
        public virtual ICollection<CarreraMateria> CarreraMaterias { get; set; } = new List<CarreraMateria>();

        // Propiedades calculadas
        [NotMapped]
        [Display(Name = "Carrera Completa")]
        public string CarreraCompleta => $"{Codigo} - {Nombre}";

        [NotMapped]
        [Display(Name = "Total de Estudiantes")]
        public int TotalEstudiantes => Estudiantes?.Count(e => e.Activo) ?? 0;

        [NotMapped]
        [Display(Name = "Total de Materias")]
        public int TotalMaterias => CarreraMaterias?.Count ?? 0;
    }
}