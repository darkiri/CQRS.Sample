using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CQRS.Sample.GUI.Models
{
    public class CreateAccountViewModel
    {
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Not a valid email")]
        public string Email { get; set; }

        [DisplayName("Password")]
        [Required(ErrorMessage = "Required")]
        public string Password1 { get; set; }

        [DisplayName("Repeat password")]
        [Required(ErrorMessage = "Required")]
        [Compare("Password1", ErrorMessage = "Enter same password twice.")]
        public string Password2 { get; set; }
    }

    public class LoginAccountViewModel
    {
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Password { get; set; }
    }
}