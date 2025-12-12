using AssignmentCore.Models;
using AssignmentCore.Repositories;
using AssignmentCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssignmentCore.Controllers
{
    [Authorize]
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
            var vm = new AdminDashboardViewModel();

            var role = User.FindFirst(ClaimTypes.Role)!.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (role == "Admin")
            {
                // Courses
                var allCourses = await _courseRepository.GetAllAsync();
                vm.TotalCourses = allCourses.Count;

                // Assignments
                vm.TotalAssignments = await _assignmentRepository.CountAsync();
                vm.ActiveAssignments = await _assignmentRepository.CountActiveAsync();

                // Users
                var allUsers = await _userRepository.GetAllAsync();
                vm.TotalUsers = allUsers.Count;

                // Latest assignments
                var latest = await _assignmentRepository.GetLatestWithCourseAsync(5);
                vm.LatestAssignments = latest.Select(a => new AdminDashboardAssignmentItem
                {
                    Title = a.Title,
                    CourseName = a.Course?.Name ?? "-",
                    CreatedAt = a.CreatedAt,
                    DueDate = a.DueDate,
                    IsActive = a.IsActive
                }).ToList();
            }
            else if (role == "Teacher")
            {
                // Bu öğretmenin dersleri
                var teacherCourses = await _courseRepository.GetByTeacherAsync(userId);
                vm.TotalCourses = teacherCourses.Count;

                // Assignments
                vm.TotalAssignments = await _assignmentRepository.CountByTeacherAsync(userId);
                vm.ActiveAssignments = await _assignmentRepository.CountActiveByTeacherAsync(userId);

                // TotalUsers: istersen 0 bırak, istersen tüm öğrencileri say
                vm.TotalUsers = 0;

                // Latest assignments (sadece kendi derslerinden)
                var latest = await _assignmentRepository.GetLatestForTeacherAsync(userId, 5);
                vm.LatestAssignments = latest.Select(a => new AdminDashboardAssignmentItem
                {
                    Title = a.Title,
                    CourseName = a.Course?.Name ?? "-",
                    CreatedAt = a.CreatedAt,
                    DueDate = a.DueDate,
                    IsActive = a.IsActive
                }).ToList();
            }
            else if (role == "Student")
            {
                // Öğrencinin atandığı kurslar
                var studentCourses = await _courseRepository.GetForStudentAsync(userId);
                vm.TotalCourses = studentCourses.Count;

                // Öğrenciye ait ödev sayıları
                vm.TotalAssignments = await _assignmentRepository.CountForStudentAsync(userId);
                vm.ActiveAssignments = await _assignmentRepository.CountActiveForStudentAsync(userId);

                vm.TotalUsers = 0; // öğrenci için istersen göstermeyebilirsin

                // Latest assignments: işte burası senin istediğin kısım
                var latest = await _assignmentRepository.GetLatestForStudentAsync(userId, 5);
                vm.LatestAssignments = latest.Select(a => new AdminDashboardAssignmentItem
                {
                    Title = a.Title,
                    CourseName = a.Course?.Name ?? "-",
                    CreatedAt = a.CreatedAt,
                    DueDate = a.DueDate,
                    IsActive = a.IsActive
                }).ToList();
            }
            else
            {
                // rol tanımsızsa boş dashboard
                vm.TotalCourses = 0;
                vm.TotalAssignments = 0;
                vm.ActiveAssignments = 0;
                vm.TotalUsers = 0;
            }

            return View(vm);
        }
    }

}
