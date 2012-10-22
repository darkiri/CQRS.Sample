using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CQRS.Sample.GUI.Models
{
    public class CreateAccountViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.Date)]
        [EmailAddress(ErrorMessage = "Not a valid email")]
        public string Email { get; set; }

        [DisplayName("Password")]
        [Required(ErrorMessage = "Required")]
        public string Password1 { get; set; }

        [DisplayName("Repeat password")]
        [Required(ErrorMessage = "Required")]
        public string Password2 { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password1 == Password2)
            {
                yield return ValidationResult.Success;
            }
            else
            {
                yield return new ValidationResult("Enter same password twice", new[] {"Password1", "Password2"});
            }
        }
    }
}