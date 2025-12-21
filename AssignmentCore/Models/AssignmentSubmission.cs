using System.ComponentModel.DataAnnotations;

namespace AssignmentCore.Models
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; } = null!;

        public int StudentId { get; set; }
        public User Student { get; set; } = null!;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Uploaded file info
        public string FilePath { get; set; } = null!;
        public string OriginalName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long Size { get; set; }

        public string? Note { get; set; }

        public int? Grade { get; set; }
        public string? TeacherFeedback { get; set; }
        public DateTime? GradedAt { get; set; }
        public int? GradedByUserId { get; set; }
    }
}
