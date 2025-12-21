using AssignmentCore.Data;
using AssignmentCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AssignmentCore.Repositories
{
    public class AssignmentRepository:GenericRepository<Assignment>
    {
        public AssignmentRepository(ApplicationDbContext context): base(context)
        {

        }

        public async Task<List<Assignment>> GetLatestWithCourseAsync(int count)
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Assignment>> GetLatestForTeacherAsync(int teacherId, int count)
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.Course.TeacherId == teacherId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Assignment>> GetLatestForStudentAsync(int studentId, int count)
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .Where(a =>
                    a.Course.Students.Any(cs =>
                        cs.StudentId == studentId &&
                        cs.IsActive))
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Assignment>> GetAllWithCourseAsync()
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .ToListAsync();
        }

        public async Task<List<Assignment>> GetForTeacherWithCourseAsync(int teacherId)
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.Course.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<List<Assignment>> GetForStudentWithCourseAsync(int studentId)
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.Course.Students
                    .Any(cs => cs.StudentId == studentId && cs.IsActive))
                .ToListAsync();
        }

        public async Task<Assignment?> GetByIdWithCourseAsync(int id)
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<int> CountAsync()
            => await _context.Assignments.CountAsync();

        public async Task<int> CountActiveAsync()
            => await _context.Assignments.CountAsync(a => a.IsActive);

        public async Task<int> CountByTeacherAsync(int teacherId)
            => await _context.Assignments.CountAsync(a => a.Course.TeacherId == teacherId);

        public async Task<int> CountActiveByTeacherAsync(int teacherId)
            => await _context.Assignments.CountAsync(a => a.IsActive && a.Course.TeacherId == teacherId);

        public async Task<int> CountForStudentAsync(int studentId)
            => await _context.Assignments.CountAsync(a =>
                a.Course.Students.Any(cs => cs.StudentId == studentId && cs.IsActive));

        public async Task<int> CountActiveForStudentAsync(int studentId)
            => await _context.Assignments.CountAsync(a =>
                a.IsActive &&
                a.Course.Students.Any(cs => cs.StudentId == studentId && cs.IsActive));

        public async Task<Assignment?> GetByIdWithCourseAndStudentsAsync(int id)
        {
            return await _context.Assignments
                .Include(a => a.Course)
                    .ThenInclude(c => c.Students)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}
