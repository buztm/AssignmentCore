using AspNetCoreHero.ToastNotification.Abstractions;
using AssignmentCore.Models;
using AssignmentCore.Repositories;
using AssignmentCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AssignmentCore.Controllers
{
    [Authorize]
    public class AssignmentController : Controller
    {
        private readonly AssignmentRepository _assignmentRepository;
        private readonly CourseRepository _courseRepository;
        private readonly UserRepository _userRepository;
        private readonly INotyfService _notyf;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AssignmentController(
            AssignmentRepository assignmentRepository,
            CourseRepository courseRepository,
            UserRepository userRepository, 
            INotyfService notyf, 
            IHubContext<NotificationHub> hubContext)
        {
            _assignmentRepository = assignmentRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _notyf = notyf;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["title"] = "Assignments";
            ViewData["subTitle"] = "Assignment List";

            var role = User.FindFirst(ClaimTypes.Role)!.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            List<Assignment> assignments;

            if (role == "Admin")
            {
                assignments = await _assignmentRepository.GetAllWithCourseAsync();
            }
            else if (role == "Teacher")
            {
                assignments = await _assignmentRepository.GetForTeacherWithCourseAsync(userId);
            }
            else if (role == "Student")
            {
                assignments = await _assignmentRepository.GetForStudentWithCourseAsync(userId);
            }
            else
            {
                assignments = new List<Assignment>();
            }

            return View(assignments);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Create()
        {
            ViewData["title"] = "Add Assignment";
            ViewData["subTitle"] = "New Assignment";

            var vm = new AssignmentCreateViewModel();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            List<Course> courses;

            if (role == "Admin")
            {
                courses = await _courseRepository.GetAllAsync();
            }
            else
            {
                courses = await _courseRepository.GetByTeacherAsync(userId);
            }

            vm.Courses = courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            return View(vm);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost]
        public async Task<IActionResult> Create(AssignmentCreateViewModel model)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            var userId = int.Parse(userIdStr);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            List<Course> courses;
            if (role == "Admin")
            {
                courses = await _courseRepository.GetAllAsync();
            }
            else
            {
                courses = await _courseRepository.GetByTeacherAsync(userId);
            }

            if (!ModelState.IsValid)
            {
                model.Courses = courses.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

                return View(model);
            }

            if (role == "Teacher")
            {
                var canUseCourse = courses.Any(c => c.Id == model.CourseId);
                if (!canUseCourse)
                {
                    return Forbid();
                }
            }

            var assignment = new Assignment
            {
                Title = model.Title,
                Description = model.Description,
                CourseId = model.CourseId,
                DueDate = model.DueDate,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = model.IsActive
            };

            await _assignmentRepository.AddAsync(assignment);
            await _assignmentRepository.SaveAsync();

            _notyf.Success("Assignment created successfully");

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(int id)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            ViewData["title"] = "Edit Assignment";
            ViewData["subTitle"] = "Update Assignment";

            var courses = await _courseRepository.GetAllAsync();

            var vm = new AssignmentCreateViewModel
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                CourseId = assignment.CourseId,
                DueDate = assignment.DueDate,
                IsActive = assignment.IsActive,
                Courses = courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList()
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, AssignmentCreateViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                var courses = await _courseRepository.GetAllAsync();
                model.Courses = courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList();

                return View(model);
            }

            var assignment = await _assignmentRepository.GetByIdAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            assignment.Title = model.Title;
            assignment.Description = model.Description;
            assignment.CourseId = model.CourseId;
            assignment.DueDate = model.DueDate;
            assignment.IsActive = model.IsActive;

            _assignmentRepository.Update(assignment);
            await _assignmentRepository.SaveAsync();

            _notyf.Information("Assignment edited successfully");

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            ViewData["title"] = "Delete Assignment";
            ViewData["subTitle"] = "Confirm Assignment";

            return View(assignment);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            _assignmentRepository.Remove(assignment);
            await _assignmentRepository.SaveAsync();

            _notyf.Warning("Assignment deleted successfully");

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(id);
            if (assignment == null)
            {
                return Json(new { success = false });
            }

            assignment.IsActive = !assignment.IsActive;

            _assignmentRepository.Update(assignment);
            await _assignmentRepository.SaveAsync();

            return Json(new
            {
                success = true,
                isActive = assignment.IsActive
            });
        }
    }
}
