using AssignmentCore.Models;
using AssignmentCore.Repositories;
using AssignmentCore.Security;
using AssignmentCore.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentCore.Controllers
{
    public class UserController : Controller
    {
        private readonly UserRepository _userRepository;

        public UserController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Teachers()
        {
            ViewData["title"] = "Teachers";
            ViewData["subTitle"] = "Teacher List";

            var users = await _userRepository.GetAllAsync();
            var teachers = users.Where(u => u.Role == "Teacher").ToList();

            return View(teachers);
        }

        public async Task<IActionResult> Students()
        {
            ViewData["title"] = "Students";
            ViewData["subTitle"] = "Student List";

            var users = await _userRepository.GetAllAsync();
            var students = users.Where(u => u.Role == "Student").ToList();

            return View(students);
        }

        public IActionResult CreateTeacher()
        {
            ViewData["title"] = "Add Teacher";
            ViewData["subTitle"] = "New Teacher";

            var vm = new UserCreateViewModel
            {
                Role = "Teacher",
                IsActive = true
            };

            return View("Create", vm);
        }

        public IActionResult CreateStudent()
        {
            ViewData["title"] = "Add Student";
            ViewData["subTitle"] = "New Student";

            var vm = new UserCreateViewModel
            {
                Role = "Student",
                IsActive = true
            };

            return View("Create", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (hash, salt) = PasswordHelper.CreatePasswordHash(model.Password);

            var user = new User
            {
                UserName = model.UserName,
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = model.Role,
                IsActive = model.IsActive
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();

            // Role'e göre listeye geri dön
            if (model.Role == "Teacher")
                return RedirectToAction("Teachers");
            else if (model.Role == "Student")
                return RedirectToAction("Students");
            else
                return RedirectToAction("Teachers");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            ViewData["title"] = "Edit User";
            ViewData["subTitle"] = user.Role == "Teacher" ? "Update Teacher" : "Update Student";

            var vm = new UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            user.UserName = model.UserName;
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            // Şifre alanı doluysa MD5+Salt ile güncelle
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var (hash, salt) = PasswordHelper.CreatePasswordHash(model.Password);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
            }

            _userRepository.Update(user);
            await _userRepository.SaveAsync();

            if (model.Role == "Teacher")
                return RedirectToAction("Teachers");
            else
                return RedirectToAction("Students");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            ViewData["title"] = "Delete User";
            ViewData["subTitle"] = user.Role == "Teacher" ? "Delete Teacher" : "Delete Student";

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var role = user.Role;

            _userRepository.Remove(user);
            await _userRepository.SaveAsync();

            if (role == "Teacher")
                return RedirectToAction("Teachers");
            else
                return RedirectToAction("Students");
        }
    }
}
