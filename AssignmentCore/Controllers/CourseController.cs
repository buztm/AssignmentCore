using AspNetCoreHero.ToastNotification.Abstractions;
using AssignmentCore.Hubs;
using AssignmentCore.Models;
using AssignmentCore.Repositories;
using AssignmentCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AssignmentCore.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly CourseRepository _courseRepository;
        private readonly UserRepository _userRepository;
        private readonly INotyfService _notyf;
        private readonly IHubContext<GeneralHub> _hubContext;

        public CourseController(CourseRepository courseRepository, UserRepository userRepository, 
                                INotyfService notyf, IHubContext<GeneralHub> hubContext)
        {
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _notyf = notyf;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            List<Course> courses;

            if (role == "Admin")
            {
                courses = await _courseRepository.GetAllWithTeacherAsync();
            }
            else if (role == "Teacher")
            {
                courses = await _courseRepository.GetByTeacherAsync(userId);
            }
            else if (role == "Student")
            {
                courses = await _courseRepository.GetForStudentAsync(userId);
            }
            else
            {
                courses = new List<Course>();
            }

            return View(courses);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentIndex(string? search)
        {
            ViewData["title"] = "My Courses";
            ViewData["subTitle"] = "Your enrolled courses";

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var courses = await _courseRepository.GetForStudentAsync(userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                courses = courses.Where(c =>
                    (c.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (c.Code ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return View(courses);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DetailsStudent(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var course = await _courseRepository.GetByIdWithStudentsAndAssignmentsAsync(id);
            if (course == null) return NotFound();

            var enrolled = course.Students.Any(s => s.StudentId == userId && s.IsActive);
            if (!enrolled) return Forbid();

            ViewData["title"] = "Course Details";
            ViewData["subTitle"] = $"{course.Code} • {course.Name}";
            return View(course);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Create()
        {
            ViewData["title"] = "Add Course";
            ViewData["subTitle"] = "New Course";

            if (User.IsInRole("Admin"))
                await LoadTeachersAsync();

            return View(new Course());
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost]
        public async Task<IActionResult> Create(Course course)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            if (role == "Teacher")
            {
                course.TeacherId = userId;
                ModelState.Remove("TeacherId");
            }

            if (role == "Admin" && course.TeacherId == 0)
            {
                ModelState.AddModelError("TeacherId", "Please select a teacher.");
            }

            if (!ModelState.IsValid)
            {
                if (role == "Admin")
                    await LoadTeachersAsync();

                return View(course);
            }

            course.CreatedAt = DateTime.UtcNow;

            await _courseRepository.AddAsync(course);
            await _courseRepository.SaveAsync();

            _notyf.Success("Course created successfully.");

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
                return NotFound();

            ViewData["title"] = "Edit Course";
            ViewData["subTitle"] = "Update Course";

            if (User.IsInRole("Admin"))
                await LoadTeachersAsync();

            return View(course);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Course model)
        {
            if (id != model.Id)
                return BadRequest();

            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                    await LoadTeachersAsync();

                return View(model);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            if (role != "Admin" && course.TeacherId != userId)
                return Forbid();

            course.Code = model.Code;
            course.Name = model.Name;
            course.Description = model.Description;
            course.IsActive = model.IsActive;

            if (role == "Admin")
            {
                course.TeacherId = model.TeacherId;
            }

            _courseRepository.Update(course);
            await _courseRepository.SaveAsync();

            _notyf.Information("Course edited successfully");

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseRepository.GetByIdWithStudentsAndAssignmentsAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            ViewData["title"] = "Delete Course";
            ViewData["subTitle"] = "Confirm Delete";

            ViewBag.HasAssignments = course.Assignments.Any();
            ViewBag.HasStudents = course.Students.Any(cs => cs.IsActive);

            return View(course);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _courseRepository.Remove(course);
            await _courseRepository.SaveAsync();

            _notyf.Warning("Course deleted successfully");

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                return Json(new { success = false });
            }

            course.IsActive = !course.IsActive;

            _courseRepository.Update(course);
            await _courseRepository.SaveAsync();

            return Json(new
            {
                success = true,
                isActive = course.IsActive
            });
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet]
        public async Task<IActionResult> AssignStudents(int id)
        {
            var course = await _courseRepository.GetByIdWithStudentsAsync(id);
            if (course == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            if (role != "Admin" && course.TeacherId != userId)
                return Forbid();

            var students = await _userRepository.GetActiveStudentsAsync();

            var vm = new CourseAssignStudentsViewModel
            {
                CourseId = course.Id,
                CourseName = course.Name,
                SelectedStudentIds = course.Students
                    .Where(cs => cs.IsActive)
                    .Select(cs => cs.StudentId)
                    .ToList(),
                Students = students.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.FullName} ({s.UserName})"
                }).ToList()
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost]
        public async Task<IActionResult> AssignStudents(CourseAssignStudentsViewModel model)
        {
            var course = await _courseRepository.GetByIdWithStudentsAsync(model.CourseId);
            if (course == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            if (role != "Admin" && course.TeacherId != userId)
                return Forbid();

            var selectedIds = model.SelectedStudentIds?.ToHashSet() ?? new HashSet<int>();

            foreach (var cs in course.Students)
            {
                cs.IsActive = selectedIds.Contains(cs.StudentId);
            }

            var existingIds = course.Students.Select(cs => cs.StudentId).ToHashSet();

            foreach (var sid in selectedIds)
            {
                if (!existingIds.Contains(sid))
                {
                    course.Students.Add(new CourseStudent
                    {
                        CourseId = course.Id,
                        StudentId = sid,
                        IsActive = true
                    });
                }
            }

            await _courseRepository.SaveAsync();

            _notyf.Information("Students assigned to the course successfully");

            TempData["AssignSuccess"] = "Students assigned successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadTeachersAsync()
        {
            var teachers = await _userRepository.GetActiveAdminAndTeacherAsync();

            ViewBag.Teachers = teachers.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"{t.FullName} ({t.UserName})"
            }).ToList();
        }
    }
}
