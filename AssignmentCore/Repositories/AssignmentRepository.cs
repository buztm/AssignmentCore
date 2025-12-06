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

        // İstersen ekstra metotlar da yazabilirsin
        public async Task<List<Assignment>> GetAllWithCourseAsync()
        {
            return await _dbSet
                .Include(a => a.Course)
                .Include(a => a.CreatedByUser)
                .ToListAsync();
        }
    }
}
