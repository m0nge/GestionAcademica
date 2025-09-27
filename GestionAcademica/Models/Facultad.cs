using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionAcademica.Models
{
    public class Facultad
    {
        [Key]
        public int FacultadID { get; set; }

        [Required(ErrorMessage = "El nombre de la facultad es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre de la Facultad")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(10, ErrorMessage = "El código no puede exceder 10 caracteres")]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        // Navegación hacia carreras
        public virtual ICollection<Carrera> Carreras { get; set; } = new List<Carrera>();

        // Navegación hacia docentes
        public virtual ICollection<Docente> Docentes { get; set; } = new List<Docente>();
    }
}