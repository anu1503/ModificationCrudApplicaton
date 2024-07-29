
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using static ModifiedCrudApp.Controllers.HomeController;

namespace ModifiedCrudApp.Models
{
    public class AlphabeticAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var strValue = value.ToString();
                // Update the regular expression to allow spaces along with alphabetic characters
                if (!Regex.IsMatch(strValue, @"^[a-zA-Z\s]+$"))
                {
                    return new ValidationResult(ErrorMessage ?? "The field should contain only alphabetic characters and spaces.");
                }
            }
            return ValidationResult.Success;
        }
    }
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmpNo { get; set; }

        [Required]
        [Alphabetic(ErrorMessage = "The name should contain only alphabetic characters.")]
        public string EmpName { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@".*\.com$", ErrorMessage = "Email must end with .com")]
        public string Email
        {
            get => _email;
            set => _email = value?.ToLower(); // Convert email to lowercase
        }

        public string Gender { get; set; }

        public string Country { get; set; }

        public string State { get; set; }

        private string _email;
    }



}
