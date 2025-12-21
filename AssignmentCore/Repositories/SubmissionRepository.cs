using AssignmentCore.Data;
using AssignmentCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AssignmentCore.Repositories
{
    public class SubmissionRepository : GenericRepository<AssignmentSubmission>
    {
        public SubmissionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<AssignmentSubmission?> GetByAssignmentAndStudentAsync(int assignmentId, int studentId)
        {
            return await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);
        }

        public async Task<List<AssignmentSubmission>> GetForAssignmentAsync(int assignmentId)
        {
            return await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Where(s => s.AssignmentId == assignmentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }
    }
}