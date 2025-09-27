using System.ComponentModel.DataAnnotations;

namespace GestionAcademica.Models
{
    public class YearRangeAttribute : ValidationAttribute
    {
        private readonly int _minYear;
        private readonly int _maxYear;

        public YearRangeAttribute(int minYear, int maxYear = 0)
        {
            _minYear = minYear;
            _maxYear = maxYear == 0 ? DateTime.Now.Year : maxYear;
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
                return true; // Permitir null, usar Required para obligatoriedad

            if (value is DateTime dateTime)
            {
                var year = dateTime.Year;
                return year >= _minYear && year <= _maxYear;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessage ?? "El año debe estar entre {0} y {1}", _minYear, _maxYear);
        }
    }
}