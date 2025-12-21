namespace AssignmentCore.ViewModels
{
    public class StudentAssignmentItemVM
    {
        public AssignmentCore.Models.Assignment Assignment { get; set; } = null!;
        public bool IsSubmitted { get; set; }
        public int? SubmissionId { get; set; }
        public bool IsLate { get; set; }
    }
}
