using System.ComponentModel.DataAnnotations;

namespace AssignmentCore.Models
{
    public class Course
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Course Name is required.")]
        public string Name { get; set; } = null!;
        [Required(ErrorMessage = "Course Code is required.")]
        public string Code { get; set; } = null!;
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int TeacherId { get; set; }
        public User? Teacher { get; set; }

        public ICollection<CourseStudent> Students { get; set; } = new List<CourseStudent>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
