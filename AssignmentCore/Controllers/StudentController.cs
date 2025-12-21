using AssignmentCore.Repositories;
using AssignmentCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssignmentCore.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly CourseRepository _courseRepository;
        private readonly AssignmentRepository _assignmentRepository;

        public StudentController(CourseRepository courseRepository, AssignmentRepository assignmentRepository)
        {
            _courseRepository = courseRepository;
            _assignmentRepository = assignmentRepository;
        }

        public async Task<IActionResult> Overview()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var vm = new StudentOverviewViewModel();

            var studentCourses = await _courseRepository.GetForStudentAsync(userId);
            vm.TotalCourses = studentCourses.Count;

            vm.TotalAssignments = await _assignmentRepository.CountForStudentAsync(userId);
            vm.ActiveAssignments = await _assignmentRepository.CountActiveForStudentAsync(userId);

            var list = await _assignmentRepository.GetLatestForStudentAsync(userId, 12);

            var today = DateTime.UtcNow.Date;
            var dueSoonLimit = today.AddDays(7);

            vm.OverdueCount = list.Count(a => a.DueDate != null && a.IsActive && a.DueDate.Value.Date < today);
            vm.DueSoonCount = list.Count(a => a.DueDate != null && a.IsActive && a.DueDate.Value.Date >= today && a.DueDate.Value.Date <= dueSoonLimit);

            vm.UpcomingAssignments = list
                .Where(a => a.DueDate != null)
                .OrderBy(a => a.DueDate)
                .Take(6)
                .Select(a => new StudentOverviewAssignmentItem
                {
                    Id = a.Id,
                    Title = a.Title,
                    CourseName = a.Course?.Name ?? "-",
                    CreatedAt = a.CreatedAt,
                    DueDate = a.DueDate,
                    IsActive = a.IsActive
                })
                .ToList();

            ViewData["title"] = "Overview";
            ViewData["subTitle"] = "Your quick snapshot";
            return View(vm);
        }
    }
}
