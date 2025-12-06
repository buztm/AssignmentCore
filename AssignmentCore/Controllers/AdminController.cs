using AssignmentCore.Models;
using AssignmentCore.Repositories;
using AssignmentCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssignmentCore.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class AdminController : Controller
    {
        private readonly CourseRepository _courseRepository;
        private readonly AssignmentRepository _assignmentRepository;
        private readonly UserRepository _userRepository;

        public AdminController(
            CourseRepository courseRepository,
            AssignmentRepository assignmentRepository,
            UserRepository userRepository)
        {
            _courseRepository = courseRepository;
            _assignmentRepository = assignmentRepository;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            List<Course> courses;
            List<Assignment> assignments;

            if (role == "Admin")
            {
                courses = await _courseRepository.GetAllAsync();
                assignments = await _assignmentRepository.GetAllWithCourseAsync();
            }
            else if (role == "Teacher")
            {
                courses = await _courseRepository.GetByTeacherAsync(userId);
                assignments = (await _assignmentRepository.GetAllWithCourseAsync())
                    .Where(a => a.CreatedByUserId == userId)
                    .ToList();
            }
            else if (role == "Student")
            {
                courses = await _courseRepository.GetForStudentAsync(userId);
                assignments = (await _assignmentRepository.GetAllWithCourseAsync())
                    .Where(a => a.Course.Students.Any(cs => cs.StudentId == userId && cs.IsActive))
                    .ToList();
            }
            else
            {
                courses = new List<Course>();
                assignments = new List<Assignment>();
            }

            var users = await _userRepository.GetAllAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalCourses = courses.Count,
                TotalAssignments = assignments.Count,
                ActiveAssignments = assignments.Count(a => a.IsActive),
                TotalUsers = users.Count(u => u.IsActive),

                LatestAssignments = assignments
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .Select(a => new AdminDashboardAssignmentItem
                    {
                        Title = a.Title,
                        CourseName = a.Course.Name,
                        CreatedAt = a.CreatedAt,
                        DueDate = a.DueDate
                    }).ToList()
            };

            return View(vm);
        }
    }

}
