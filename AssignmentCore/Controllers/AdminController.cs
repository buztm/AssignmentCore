using AssignmentCore.Repositories;
using AssignmentCore.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentCore.Controllers
{
    using AssignmentCore.Repositories;
    using AssignmentCore.ViewModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

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
            ViewData["title"] = "Admin Dashboard";
            ViewData["subTitle"] = "Genel Bakış";

            var courses = await _courseRepository.GetAllAsync();
            var assignments = await _assignmentRepository.GetAllWithCourseAsync();
            var users = await _userRepository.GetAllAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalCourses = courses.Count,
                TotalAssignments = assignments.Count,
                ActiveAssignments = assignments.Count(a => a.IsActive),
                TotalUsers = users.Count,
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
