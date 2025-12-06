using AssignmentCore.Data;
using AssignmentCore.Models;
using AssignmentCore.Repositories;
using Microsoft.EntityFrameworkCore;

public class CourseRepository : GenericRepository<Course>
{
    private readonly ApplicationDbContext _context;

    public CourseRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<List<Course>> GetByTeacherAsync(int teacherId)
    {
        return _dbSet
            .Where(c => c.TeacherId == teacherId && c.IsActive)
            .ToListAsync();
    }

    public Task<List<Course>> GetForStudentAsync(int studentId)
    {
        return _context.CourseStudents
            .Where(cs => cs.StudentId == studentId
                         && cs.IsActive
                         && cs.Course.IsActive)
            .Select(cs => cs.Course)
            .Distinct()
            .ToListAsync();
    }

    public Task<Course?> GetByIdWithStudentsAsync(int id)
    {
        return _dbSet
            .Include(c => c.Students)
                .ThenInclude(cs => cs.Student)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
