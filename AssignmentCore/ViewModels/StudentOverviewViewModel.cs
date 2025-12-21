namespace AssignmentCore.ViewModels
{
    public class StudentOverviewViewModel
    {
        public int TotalCourses { get; set; }
        public int TotalAssignments { get; set; }
        public int ActiveAssignments { get; set; }

        public int DueSoonCount { get; set; }
        public int OverdueCount { get; set; }

        public List<StudentOverviewAssignmentItem> UpcomingAssignments { get; set; } = new();
    }

    public class StudentOverviewAssignmentItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string CourseName { get; set; } = "-";
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsActive { get; set; }
    }
}
