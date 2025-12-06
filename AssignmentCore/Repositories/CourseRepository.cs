using AssignmentCore.Data;
using AssignmentCore.Models;

namespace AssignmentCore.Repositories
{
    public class CourseRepository:GenericRepository<Course>
    {
        public CourseRepository(ApplicationDbContext context): base(context)
        {
        }
    }
}
