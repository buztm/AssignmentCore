using AspNetCoreHero.ToastNotification.Abstractions;
using AssignmentCore.Hubs;
using AssignmentCore.Models;
using AssignmentCore.Repositories;
using AssignmentCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using System.Data;
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
        private readonly IHubContext<GeneralHub> _hubContext;
        private readonly IWebHostEnvironment _env;
        private readonly SubmissionRepository _submissionRepository;

        public AssignmentController(
            AssignmentRepository assignmentRepository,
            CourseRepository courseRepository,
            UserRepository userRepository,
            INotyfService notyf,
            IHubContext<GeneralHub> hubContext,
            IWebHostEnvironment env,
            SubmissionRepository submissionRepository)
        {
            _assignmentRepository = assignmentRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _notyf = notyf;
            _hubContext = hubContext;
            _env = env;
            _submissionRepository = submissionRepository;
        }

        public async Task<IActionResult> Index(string? search, string status = "all", int? courseId = null)
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

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentIndex(string? search, string status = "all", int? courseId = null)
        {
            ViewData["title"] = "My Assignments";
            ViewData["subTitle"] = "Track deadlines, files, and details";

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var assignments = await _assignmentRepository.GetForStudentWithCourseAsync(userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                assignments = assignments.Where(a =>
                    (a.Title ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (a.Description ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (a.Course?.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (a.Course?.Code ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (courseId.HasValue)
                assignments = assignments.Where(a => a.CourseId == courseId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                if (status == "active") assignments = assignments.Where(a => a.IsActive).ToList();
                if (status == "inactive") assignments = assignments.Where(a => !a.IsActive).ToList();
            }

            var courseItems = assignments
                .Where(a => a.Course != null)
                .Select(a => a.Course!)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Code} • {c.Name}"
                })
                .ToList();

            var vm = new StudentAssignmentsPageViewModel
            {
                Assignments = assignments.OrderBy(a => a.DueDate ?? DateTime.MaxValue).ToList(),
                Search = search,
                Status = status,
                CourseId = courseId,
                Courses = courseItems
            };

            return View(vm);
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

            var uploadInfo = await SaveAttachmentAsync(model.Attachment);

            var assignment = new Assignment
            {
                Title = model.Title,
                Description = model.Description,
                CourseId = model.CourseId,
                DueDate = model.DueDate,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = model.IsActive,
                AttachmentPath = uploadInfo?.relPath,
                AttachmentOriginalName = uploadInfo?.originalName,
                AttachmentContentType = uploadInfo?.contentType,
                AttachmentSize = uploadInfo?.size
            };

            await _assignmentRepository.AddAsync(assignment);
            await _assignmentRepository.SaveAsync();

            int totalAssignments;
            if(role == "Admin")
            {
                totalAssignments = await _assignmentRepository.CountAsync();
            }
            else if(role == "Teacher")
            {
                totalAssignments = await _assignmentRepository.CountByTeacherAsync(userId);
            }
            else
            {
                totalAssignments = await _assignmentRepository.CountForStudentAsync(userId);
            }

            await _hubContext.Clients.All.SendAsync("onAssignmentAdd", totalAssignments);

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
                ExistingAttachmentPath = assignment.AttachmentPath,
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

            if (model.Attachment != null && model.Attachment.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(assignment.AttachmentPath))
                {
                    var oldAbs = Path.Combine(_env.WebRootPath, assignment.AttachmentPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(oldAbs))
                        System.IO.File.Delete(oldAbs);
                }

                var uploadInfo = await SaveAttachmentAsync(model.Attachment);
                assignment.AttachmentPath = uploadInfo?.relPath;
                assignment.AttachmentOriginalName = uploadInfo?.originalName;
                assignment.AttachmentContentType = uploadInfo?.contentType;
                assignment.AttachmentSize = uploadInfo?.size;
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

            if (!string.IsNullOrWhiteSpace(assignment.AttachmentPath))
            {
                var abs = Path.Combine(_env.WebRootPath, assignment.AttachmentPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(abs))
                    System.IO.File.Delete(abs);
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

        private async Task<(string relPath, string originalName, string contentType, long size)?> SaveAttachmentAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var allowed = new[] { ".pdf", ".doc", ".docx", ".zip", ".rar", ".png", ".jpg", ".jpeg", ".mp4", ".mkv" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                throw new InvalidOperationException("Bu dosya türüne izin verilmiyor.");

            const long maxBytes = 500 * 1024 * 1024;
            if (file.Length > maxBytes)
                throw new InvalidOperationException("Dosya boyutu çok büyük (max 25MB).");

            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "assignments");
            Directory.CreateDirectory(uploadsRoot);

            var safeFileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{ext}";
            var absPath = Path.Combine(uploadsRoot, safeFileName);

            using (var stream = System.IO.File.Create(absPath))
                await file.CopyToAsync(stream);

            var relPath = $"/uploads/assignments/{safeFileName}";
            return (relPath, file.FileName, file.ContentType, file.Length);
        }

        public async Task<IActionResult> Details(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)!.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var assignment = await _assignmentRepository.GetByIdWithCourseAndStudentsAsync(id);
            if (assignment == null) return NotFound();

            if (role == "Teacher")
            {
                if (assignment.Course.TeacherId != userId) return Forbid();
            }
            else if (role == "Student")
            {
                var enrolled = assignment.Course.Students.Any(s => s.StudentId == userId && s.IsActive);
                if (!enrolled) return Forbid();
            }

            ViewData["title"] = "Assignment Details";
            ViewData["subTitle"] = assignment.Title;

            var submission = await _submissionRepository.GetByAssignmentAndStudentAsync(id, userId);
            ViewBag.MySubmission = submission;

            var canSubmit = assignment.IsActive
                && (assignment.DueDate == null || assignment.DueDate.Value >= DateTime.UtcNow)
                && submission == null;

            ViewBag.CanSubmit = canSubmit;

            return View(assignment);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Submissions(int id)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value;
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var assignment = await _assignmentRepository.GetByIdWithCourseAsync(id);
            if (assignment == null) return NotFound();

            if (role == "Teacher")
            {
                if (assignment.Course.TeacherId != userId) return Forbid();
            }

            var list = await _submissionRepository.GetForAssignmentAsync(id);

            ViewData["title"] = "Submissions";
            ViewData["subTitle"] = assignment.Title;

            ViewBag.Assignment = assignment;
            return View(list);
        }


        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ViewModels.SubmissionCreateViewModel model)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var assignment = await _assignmentRepository.GetByIdWithCourseAndStudentsAsync(model.AssignmentId);
            if (assignment == null) return NotFound();

            var enrolled = assignment.Course.Students.Any(s => s.StudentId == userId && s.IsActive);
            if (!enrolled) return Forbid();

            if (!assignment.IsActive)
            {
                TempData["error"] = "This course is not active.";
                return RedirectToAction(nameof(Details), new { id = assignment.Id });
            }

            if (assignment.DueDate != null && assignment.DueDate.Value < DateTime.UtcNow)
            {
                TempData["error"] = "You can't deliver because the deadline has passed.";
                return RedirectToAction(nameof(Details), new { id = assignment.Id });
            }

            var existing = await _submissionRepository.GetByAssignmentAndStudentAsync(assignment.Id, userId);
            if (existing != null)
            {
                TempData["error"] = "You've already submitted this assignment. You can't submit it again.";
                return RedirectToAction(nameof(Details), new { id = assignment.Id });
            }

            if (!ModelState.IsValid)
            {
                TempData["error"] = "There is an error on the delivery form.";
                return RedirectToAction(nameof(Details), new { id = assignment.Id });
            }

            var uploadInfo = await SaveSubmissionFileAsync(model.File, assignment.Id, userId);

            var submission = new Models.AssignmentSubmission
            {
                AssignmentId = assignment.Id,
                StudentId = userId,
                SubmittedAt = DateTime.UtcNow,

                FilePath = uploadInfo.relPath,
                OriginalName = uploadInfo.originalName,
                ContentType = uploadInfo.contentType,
                Size = uploadInfo.size,

                Note = model.Note
            };

            await _submissionRepository.AddAsync(submission);
            await _submissionRepository.SaveAsync();

            _notyf.Success("Assignment submitted.");
            return RedirectToAction(nameof(Details), new { id = assignment.Id });
        }

        private async Task<(string relPath, string originalName, string contentType, long size)> SaveSubmissionFileAsync(
            IFormFile file, int assignmentId, int studentId)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file selected.");

            var allowed = new[] { ".pdf", ".doc", ".docx", ".zip", ".rar", ".png", ".jpg", ".jpeg" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                throw new InvalidOperationException("This file type is not allowed.");

            const long maxBytes = 500 * 1024 * 1024;
            if (file.Length > maxBytes)
                throw new InvalidOperationException("The file size is too large.");

            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "submissions", assignmentId.ToString(), studentId.ToString());
            Directory.CreateDirectory(uploadsRoot);

            var safeFileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{ext}";
            var absPath = Path.Combine(uploadsRoot, safeFileName);

            using (var stream = System.IO.File.Create(absPath))
                await file.CopyToAsync(stream);

            var relPath = $"/uploads/submissions/{assignmentId}/{studentId}/{safeFileName}";
            return (relPath, file.FileName, file.ContentType, file.Length);
        }
    }
}
