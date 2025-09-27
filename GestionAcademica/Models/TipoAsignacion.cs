using System.ComponentModel.DataAnnotations;

namespace GestionAcademica.Models
{
    public class TipoAsignacion
    {
        [Key]
        public int TipoAsignacionID { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = "";

        [StringLength(200)]
        public string? Descripcion { get; set; }

        // Relación inversa
        public virtual ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
    }
}