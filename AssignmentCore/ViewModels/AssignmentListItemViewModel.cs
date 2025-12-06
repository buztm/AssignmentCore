namespace AssignmentCore.ViewModels
{
    public class AssignmentListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public DateTime? DueDate { get; set; }
        public bool IsActive { get; set; }
    }
}
