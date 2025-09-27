using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionAcademica.Models
{
    public class Materia
    {
        [Key]

        [Display(Name = "Carrera")]
        public int? CarreraID { get; set; }

        // Propiedad de navegación
        [ForeignKey("CarreraID")]
        public virtual Carrera? Carrera { get; set; }

        public int MateriaID { get; set; }

        [Required(ErrorMessage = "El nombre de la materia es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre de la Materia")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código de la materia es obligatorio")]
        [StringLength(10, ErrorMessage = "El código no puede exceder 10 caracteres")]
        [Display(Name = "Código de la Materia")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe asignar un docente a la materia")]
        [Display(Name = "Docente Asignado")]
        public int DocenteID { get; set; }

        [Range(1, 10, ErrorMessage = "Los créditos deben estar entre 1 y 10")]
        [Display(Name = "Créditos")]
        public int Creditos { get; set; } = 3;

        [StringLength(20, ErrorMessage = "El período no puede exceder 20 caracteres")]
        [Display(Name = "Período")]
        public string Periodo { get; set; } = string.Empty;

        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        // Navegación a Docente
        [ForeignKey("DocenteID")]
        public virtual Docente? Docente { get; set; }

        // Navegación a Asignaciones (en lugar de Tareas)
        public virtual ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();

        // Propiedades calculadas
        [NotMapped]
        [Display(Name = "Docente")]
        public string NombreDocente => Docente != null ? Docente.NombreCompleto : "Sin asignar";

        [NotMapped]
        [Display(Name = "Total de Asignaciones")]
        public int TotalAsignaciones => Asignaciones?.Count ?? 0;

        [NotMapped]
        [Display(Name = "Materia Completa")]
        public string MateriaCompleta => $"{Codigo} - {Nombre}";
    }
}