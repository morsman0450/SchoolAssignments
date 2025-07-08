using SchoolAssignments.Models;
using System.ComponentModel.DataAnnotations;

namespace SchoolAssignments.ViewModels
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Uživatelské jméno je povinné")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Uživatelské jméno musí mít 3-50 znaků")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je povinný")]
        [EmailAddress(ErrorMessage = "Neplatný email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jméno je povinné")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Příjmení je povinné")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Heslo je povinné")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Heslo musí mít alespoň 6 znaků")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potvrzení hesla je povinné")]
        [Compare(nameof(Password), ErrorMessage = "Hesla se neshodují")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Student;
    }
}
