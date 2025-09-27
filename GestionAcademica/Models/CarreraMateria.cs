using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionAcademica.Models
{
    public class CarreraMateria
    {
        [Key]
        public int CarreraMateriaID { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una carrera")]
        [Display(Name = "Carrera")]
        public int CarreraID { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una materia")]
        [Display(Name = "Materia")]
        public int MateriaID { get; set; }

        [Required(ErrorMessage = "El año es obligatorio")]
        [Range(1, 10, ErrorMessage = "El año debe estar entre 1 y 10")]
        [Display(Name = "Año")]
        public int Anio { get; set; } = 1;

        [Required(ErrorMessage = "El semestre es obligatorio")]
        [Range(1, 2, ErrorMessage = "El semestre debe ser 1 o 2")]
        [Display(Name = "Semestre")]
        public int Semestre { get; set; } = 1;

        [Display(Name = "Es Obligatoria")]
        public bool EsObligatoria { get; set; } = true;

        [Display(Name = "Fecha de Asignación")]
        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        // Navegación hacia Carrera
        [ForeignKey("CarreraID")]
        public virtual Carrera? Carrera { get; set; }

        // Navegación hacia Materia
        [ForeignKey("MateriaID")]
        public virtual Materia? Materia { get; set; }

        // Propiedades calculadas
        [NotMapped]
        [Display(Name = "Carrera")]
        public string NombreCarrera => Carrera?.Nombre ?? "Sin carrera";

        [NotMapped]
        [Display(Name = "Materia")]
        public string NombreMateria => Materia?.Nombre ?? "Sin materia";

        [NotMapped]
        [Display(Name = "Período")]
        public string Periodo => $"{Anio}° Año - {Semestre}° Semestre";

        [NotMapped]
        [Display(Name = "Tipo")]
        public string TipoMateria => EsObligatoria ? "Obligatoria" : "Electiva";

        [NotMapped]
        [Display(Name = "Descripción Completa")]
        public string DescripcionCompleta => $"{NombreCarrera} - {NombreMateria} ({Periodo})";
    }
}