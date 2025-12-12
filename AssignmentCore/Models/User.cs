using Microsoft.AspNetCore.Identity;

namespace AssignmentCore.Models
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; } = null!;

        public string Role { get; set; } = "Student";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? ProfileImagePath { get; set; } = "/images/default-profile.png";

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        public ICollection<Course> CoursesTaught { get; set; } = new List<Course>();

        public ICollection<CourseStudent> EnrolledCourses { get; set; } = new List<CourseStudent>();
    }
}
