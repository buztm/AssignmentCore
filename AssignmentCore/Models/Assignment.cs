using System.ComponentModel.DataAnnotations;

namespace AssignmentCore.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }

        public string? AttachmentPath { get; set; }
        public string? AttachmentOriginalName { get; set; }
        public string? AttachmentContentType { get; set; }
        public long? AttachmentSize { get; set; }


        public bool IsActive { get; set; } = true;

        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();

    }
}
