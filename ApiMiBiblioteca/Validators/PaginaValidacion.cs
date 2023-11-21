using System.ComponentModel.DataAnnotations;

namespace ApiMiBiblioteca.Validators
{
    public class PaginaValidacion : ValidationAttribute
    {
        public PaginaValidacion() { }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            { return new ValidationResult("El número de paginas debe incluirse");
            }
            int? paginas = value as int?;

            if (paginas <0)
            {
                return new ValidationResult("Las paginas no pueden ser negativas");
            }
            return ValidationResult.Success;

            //return base.IsValid(value, validationContext);
        }
    }
}



