using AssignmentCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssignmentCore.ViewModels
{
    public class StudentAssignmentsPageViewModel
    {
        public List<Assignment> Assignments { get; set; } = new();

        public string? Search { get; set; }
        public string Status { get; set; } = "all";
        public int? CourseId { get; set; }

        public List<SelectListItem> Courses { get; set; } = new();

        public Dictionary<int, AssignmentCore.Models.AssignmentSubmission> SubmissionMap { get; set; } = new();

    }
}
