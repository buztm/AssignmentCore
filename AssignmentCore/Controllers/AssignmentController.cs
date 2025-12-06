using AssignmentCore.Models;
using AssignmentCore.Repositories;
using AssignmentCore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssignmentCore.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly AssignmentRepository _assignmentRepository;
        private readonly CourseRepository _courseRepository;
        private readonly UserRepository _userRepository;

        public AssignmentController(
            AssignmentRepository assignmentRepository,
            CourseRepository courseRepository,
            UserRepository userRepository)
        {
            _assignmentRepository = assignmentRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["title"] = "Assignments";
            ViewData["subTitle"] = "Assignment List";

            var assignments = await _assignmentRepository.GetAllWithCourseAsync();
            return View(assignments);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["title"] = "Add Assignment";
            ViewData["subTitle"] = "New Assignment";

            var vm = new AssignmentCreateViewModel();
            var courses = await _courseRepository.GetAllAsync();

            vm.Courses = courses
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AssignmentCreateViewModel model)
        {
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

            // TODO: login sonrası aktif admin id'sini al
            int adminId = 2; // şimdilik hardcode

            var assignment = new Assignment
            {
                Title = model.Title,
                Description = model.Description,
                CourseId = model.CourseId,
                DueDate = model.DueDate,
                IsActive = model.IsActive,
                CreatedByUserId = adminId
            };

            await _assignmentRepository.AddAsync(assignment);
            await _assignmentRepository.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

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

            return RedirectToAction(nameof(Index));
        }

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

            return RedirectToAction(nameof(Index));
        }

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
