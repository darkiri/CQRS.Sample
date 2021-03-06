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
        [DataType(DataType.Password)]
        public string Password1 { get; set; }

        [DisplayName("Repeat password")]
        [Required(ErrorMessage = "Required")]
        [Compare("Password1", ErrorMessage = "Enter same password twice.")]
        [DataType(DataType.Password)]
        public string Password2 { get; set; }
    }

    public class LoginAccountViewModel
    {
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DisplayName("New password")]
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.Password)]
        public string Password1 { get; set; }

        [DisplayName("Repeat password")]
        [Required(ErrorMessage = "Required")]
        [Compare("Password1", ErrorMessage = "Enter same password twice.")]
        [DataType(DataType.Password)]
        public string Password2 { get; set; }
    }
}