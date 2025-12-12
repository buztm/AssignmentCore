using System.ComponentModel.DataAnnotations;

namespace AssignmentCore.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "User Name or Email")]
        public string UserNameOrEmail { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}