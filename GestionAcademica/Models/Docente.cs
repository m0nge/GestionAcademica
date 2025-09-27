using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionAcademica.Models
{
    public class Docente
    {
        [Key]
        [Display(Name = "Facultad")]
        public int? FacultadID { get; set; }

        [ForeignKey("FacultadID")]
        public virtual Facultad? Facultad { get; set; }

        public int DocenteID { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden exceder 100 caracteres")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(255)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contraseña { get; set; } = string.Empty;

        [Display(Name = "Rol")]
        [StringLength(20)]
        public string Rol { get; set; } = "Docente";


        // ===============================
        // NUEVOS CAMPOS PARA 2FA
        // ===============================

        [Display(Name = "2FA Habilitado")]
        public bool TwoFactorEnabled { get; set; } = false;

        [StringLength(6)]
        public string? TwoFactorCode { get; set; }

        public DateTime? TwoFactorCodeExpiry { get; set; }

        [Display(Name = "Última IP de acceso")]
        [StringLength(45)]
        public string? UltimaIP { get; set; }

        [Display(Name = "Último acceso")]
        public DateTime? UltimoAcceso { get; set; }

        [Display(Name = "Intentos fallidos")]
        public int IntentosFallidos { get; set; } = 0;

        [Display(Name = "Bloqueado hasta")]
        public DateTime? BloqueadoHasta { get; set; }

        // ===============================
        // PROPIEDADES CALCULADAS
        // ===============================

        [NotMapped]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Nombres} {Apellidos}";

        [NotMapped]
        public bool EsAdministrador => Rol == "Administrador";

        [NotMapped]
        public bool EstaBloqueado => BloqueadoHasta.HasValue && BloqueadoHasta.Value > DateTime.Now;

        [NotMapped]
        public bool CodigoValido => TwoFactorCodeExpiry.HasValue && TwoFactorCodeExpiry.Value > DateTime.Now;
    }
}