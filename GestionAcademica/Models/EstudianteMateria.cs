using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionAcademica.Models
{
    public class EstudianteMateria
    {
        [Key]
        public int EstudianteMateriaID { get; set; }

        [Required]
        [Display(Name = "Estudiante")]
        public int EstudianteID { get; set; }

        [Required]
        [Display(Name = "Materia")]
        public int MateriaID { get; set; }

        [Display(Name = "Fecha de Inscripción")]
        public DateTime FechaInscripcion { get; set; } = DateTime.Now;

        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // Propiedades de navegación
        [ForeignKey("EstudianteID")]
        public virtual Estudiante? Estudiante { get; set; }

        [ForeignKey("MateriaID")]
        public virtual Materia? Materia { get; set; }

        // Propiedades calculadas
        [NotMapped]
        [Display(Name = "Estudiante")]
        public string NombreEstudiante => Estudiante != null ? $"{Estudiante.Nombres} {Estudiante.Apellidos}" : "Sin asignar";

        [NotMapped]
        [Display(Name = "Materia")]
        public string NombreMateria => Materia != null ? $"{Materia.Codigo} - {Materia.Nombre}" : "Sin asignar";
    }
}