namespace AssignmentCore.Models
{
    public class CourseStudent
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public int StudentId { get; set; }
        public User Student { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }
}
