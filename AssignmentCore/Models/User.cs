namespace AssignmentCore.Models
{
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;
        public string PasswordSalt { get; set; } = null!;

        public string Role { get; set; } = "Student";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? ProfileImagePath { get; set; } = "/images/default-profile.png";

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        public ICollection<Course> CoursesTaught { get; set; } = new List<Course>();

        public ICollection<CourseStudent> EnrolledCourses { get; set; } = new List<CourseStudent>();
    }
}
