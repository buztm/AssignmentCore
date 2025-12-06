using System.ComponentModel.DataAnnotations;

namespace AssignmentCore.ViewModels
{
    public class UserCreateViewModel
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

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!;   // "Teacher" veya "Student"

        public bool IsActive { get; set; } = true;
    }
}
