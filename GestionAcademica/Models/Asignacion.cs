using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionAcademica.Models
{
    public class Asignacion
    {
        [Key]
        public int AsignacionID { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una materia")]
        [Display(Name = "Materia")]
        public int MateriaID { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de asignación")]
        [Display(Name = "Tipo de Asignación")]
        public int TipoAsignacionID { get; set; }

        [Required(ErrorMessage = "El nombre de la asignación es obligatorio")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        [Display(Name = "Nombre de la Asignación")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
        [Display(Name = "Porcentaje")]
        public decimal Porcentaje { get; set; } = 0;

        [Required(ErrorMessage = "La fecha de creación es obligatoria")]
        [Display(Name = "Fecha de Creación")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        [Display(Name = "Fecha de Vencimiento")]
        [DataType(DataType.DateTime)]
        public DateTime FechaVencimiento { get; set; }

        [Range(0, 100, ErrorMessage = "La nota máxima debe estar entre 0 y 100")]
        [Display(Name = "Nota Máxima")]
        public decimal NotaMaxima { get; set; } = 10;

        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        // Navegación a Materia
        [ForeignKey("MateriaID")]
        public virtual Materia? Materia { get; set; }

        // Propiedades calculadas
        [NotMapped]
        [Display(Name = "Días Restantes")]
        public int DiasRestantes
        {
            get
            {
                var dias = (FechaVencimiento - DateTime.Now).Days;
                return dias < 0 ? 0 : dias;
            }
        }

        [NotMapped]
        [Display(Name = "Estado Actual")]
        public string EstadoActual
        {
            get
            {
                if (!Activa) return "Inactiva";
                if (DateTime.Now > FechaVencimiento) return "Vencida";
                if (DiasRestantes <= 1) return "Urgente";
                return "Pendiente";
            }
        }

        [NotMapped]
        [Display(Name = "Materia y Código")]
        public string NombreMateria => Materia != null ? Materia.MateriaCompleta : "Sin materia";

        [NotMapped]
        public bool EsUrgente => DiasRestantes <= 1 && Activa && DateTime.Now <= FechaVencimiento;

        [NotMapped]
        public bool EstaVencida => DateTime.Now > FechaVencimiento && Activa;
    // AGREGAR ESTAS LÍNEAS:

// Navegación a TipoAsignacion
[ForeignKey("TipoAsignacionID")]
        public virtual TipoAsignacion? TipoAsignacion { get; set; }
    }
}