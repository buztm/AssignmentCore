namespace AssignmentCore.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalCourses { get; set; }
        public int TotalAssignments { get; set; }
        public int ActiveAssignments { get; set; }
        public int TotalUsers { get; set; }

        public List<AdminDashboardAssignmentItem> LatestAssignments { get; set; }
            = new();
    }

    public class AdminDashboardAssignmentItem
    {
        public string Title { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
