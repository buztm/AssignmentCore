using AssignmentCore.Models;
using AssignmentCore.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentCore.Controllers
{
    public class CourseController : Controller
    {
        private readonly CourseRepository _courseRepository;

        public CourseController(CourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["title"] = "Dersler";
            ViewData["subTitle"] = "Ders Listesi";

            var courses = await _courseRepository.GetAllAsync();
            return View(courses);
        }

        public IActionResult Create()
        {
            ViewData["title"] = "Add Course";
            ViewData["subTitle"] = "New Course";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Course model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _courseRepository.AddAsync(model);
            await _courseRepository.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            ViewData["title"] = "Edit Course";
            ViewData["subTitle"] = "Update Course";

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Course model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            _courseRepository.Update(model);
            await _courseRepository.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            ViewData["title"] = "Delete Course";
            ViewData["subTitle"] = "Confirm Delete";

            return View(course);
        }

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

            return RedirectToAction(nameof(Index));
        }

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
    }
}
