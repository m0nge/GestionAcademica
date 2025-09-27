using Microsoft.EntityFrameworkCore;

namespace GestionAcademica.Models
{
    public class EstudianteValidationService
    {
        private readonly GestionAcademicaContext _context;

        public EstudianteValidationService(GestionAcademicaContext context)
        {
            _context = context;
        }

        public async Task<bool> IsCarnetUniqueAsync(string carnet, int? estudianteId = null)
        {
            var query = _context.Estudiantes.Where(e => e.Carnet == carnet);

            if (estudianteId.HasValue)
            {
                query = query.Where(e => e.EstudianteID != estudianteId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? estudianteId = null)
        {
            if (string.IsNullOrEmpty(email)) return true;

            var query = _context.Estudiantes.Where(e => e.Email == email);

            if (estudianteId.HasValue)
            {
                query = query.Where(e => e.EstudianteID != estudianteId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<bool> IsTelefonoUniqueAsync(string telefono, int? estudianteId = null)
        {
            if (string.IsNullOrEmpty(telefono)) return true;

            var query = _context.Estudiantes.Where(e => e.Telefono == telefono);

            if (estudianteId.HasValue)
            {
                query = query.Where(e => e.EstudianteID != estudianteId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<ValidationResult> ValidateEstudianteAsync(Estudiante estudiante)
        {
            var result = new ValidationResult();

            if (!await IsCarnetUniqueAsync(estudiante.Carnet, estudiante.EstudianteID))
            {
                result.Errors.Add("Carnet", "Ya existe un estudiante con este carnet");
            }

            if (!await IsEmailUniqueAsync(estudiante.Email, estudiante.EstudianteID))
            {
                result.Errors.Add("Email", "Ya existe un estudiante con este email");
            }

            if (!await IsTelefonoUniqueAsync(estudiante.Telefono, estudiante.EstudianteID))
            {
                result.Errors.Add("Telefono", "Ya existe un estudiante con este teléfono");
            }

            return result;
        }
    }

    public class ValidationResult
    {
        public Dictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
        public bool IsValid => !Errors.Any();
    }
}