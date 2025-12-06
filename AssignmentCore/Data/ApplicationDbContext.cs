using AssignmentCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AssignmentCore.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {

        }

        public DbSet<Assignment> Assignments { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
    }
}
