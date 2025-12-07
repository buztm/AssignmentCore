using AssignmentCore.Data;
using AssignmentCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AssignmentCore.Repositories
{
    public class UserRepository:GenericRepository<User>
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<User>> GetActiveStudentsAsync()
        {
            return _dbSet
                .Where(u => u.Role == "Student" && u.IsActive)
                .ToListAsync();
        }

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<List<User>> GetActiveTeachersAsync()
        {
            return await _dbSet.Where(u => u.Role == "Teacher" && u.IsActive).ToListAsync();
        }

        public async Task<List<User>> GetActiveAdminAndTeacherAsync()
        {
            return await _dbSet.Where(u => u.Role == "Admin" && u.IsActive || u.Role == "Teacher" && u.IsActive).ToListAsync();
        }
    }
}
