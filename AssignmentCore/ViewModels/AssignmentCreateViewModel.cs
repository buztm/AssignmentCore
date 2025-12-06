using AssignmentCore.Validators;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AssignmentCore.ViewModels
{
    public class AssignmentCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Display(Name = "Course")]
        [Required(ErrorMessage = "Course is required.")]
        public int CourseId { get; set; }

        [Display(Name = "Due Date")]
        [Required(ErrorMessage = "Due date is required.")]
        [DueDateNotPast]
        public DateTime? DueDate { get; set; }


        public bool IsActive { get; set; } = true;

        public List<SelectListItem> Courses { get; set; } = new();
    }
}
