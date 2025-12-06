using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssignmentCore.ViewModels
{
    public class CourseAssignStudentsViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;

        public List<int> SelectedStudentIds { get; set; } = new();

        public List<SelectListItem> Students { get; set; } = new();
    }
}
