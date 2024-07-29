using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ModifiedCrudApp.Models
{
    public class Register
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Name or Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The Password you entered is Incorrect please try again")]
        public string ConfirmPassword { get; set; }

    }
}

