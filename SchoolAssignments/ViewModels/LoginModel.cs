using System.ComponentModel.DataAnnotations;
namespace SchoolAssignments.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Uživatelské jméno je povinné")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Heslo je povinné")]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
