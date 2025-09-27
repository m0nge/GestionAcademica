using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionAcademica.Models
{
    public class Inscripcion
    {
        [Key]
        public int InscripcionID { get; set; }

        [Required]
        public int EstudianteID { get; set; }

        [Required]
        public int MateriaID { get; set; }

        public DateTime FechaInscripcion { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Estado { get; set; } = "Activo";

        // Relaciones
        [ForeignKey("EstudianteID")]
        public virtual Estudiante? Estudiante { get; set; }

        [ForeignKey("MateriaID")]
        public virtual Materia? Materia { get; set; }
    }
}