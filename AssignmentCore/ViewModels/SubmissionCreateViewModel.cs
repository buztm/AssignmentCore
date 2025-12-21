using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AssignmentCore.ViewModels
{
    public class SubmissionCreateViewModel
    {
        public int AssignmentId { get; set; }

        [Required(ErrorMessage = "Please select file.")]
        public IFormFile File { get; set; } = null!;

        public string? Note { get; set; }
    }
}
