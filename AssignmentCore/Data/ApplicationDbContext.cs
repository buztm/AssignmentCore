using AssignmentCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AssignmentCore.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany(u => u.CoursesTaught)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseStudent>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.Students)
                .HasForeignKey(cs => cs.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseStudent>()
                .HasOne(cs => cs.Student)
                .WithMany(u => u.EnrolledCourses)
                .HasForeignKey(cs => cs.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Assignment> Assignments { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<CourseStudent> CourseStudents { get; set; } = null!;
    }
}
