using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionAcademica.Models
{
    public class Calificacion
    {
        [Key]
        public int CalificacionID { get; set; }

        [Required]
        public int AsignacionID { get; set; }

        [Required]
        public int EstudianteID { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal Nota { get; set; }

        public DateTime FechaCalificacion { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Relaciones
        [ForeignKey("AsignacionID")]
        public virtual Asignacion? Asignacion { get; set; }

        [ForeignKey("EstudianteID")]
        public virtual Estudiante? Estudiante { get; set; }
    }
}