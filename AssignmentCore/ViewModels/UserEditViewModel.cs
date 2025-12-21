// ViewModels/UserEditViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace AssignmentCore.ViewModels
{
    public class UserEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; } = null!;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "New Password (optional)")]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        [Required]
        public string Role { get; set; } = null!;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }
}
