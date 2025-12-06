using AssignmentCore.Models;
using AssignmentCore.Repositories;
using AssignmentCore.Security;
using AssignmentCore.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssignmentCore.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly IWebHostEnvironment _env;

        public UserController(UserRepository userRepository, IWebHostEnvironment env)
        {
            _userRepository = userRepository;
            _env = env;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Teachers()
        {
            ViewData["title"] = "Teachers";
            ViewData["subTitle"] = "Teacher List";

            var users = await _userRepository.GetAllAsync();
            var teachers = users.Where(u => u.Role == "Teacher").ToList();

            return View(teachers);
        }

        [Authorize(Roles = "Admin,Teacher")]
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

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return RedirectToAction("Login", "Account");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return RedirectToAction("Login", "Account");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound();

            var vm = new UserProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                ProfileImagePath = user.ProfileImagePath
            };

            return View(vm); // Views/User/Profile.cshtml
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model, IFormFile? profileImageFile)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            var user = await _userRepository.GetByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            user.UserName = model.UserName;
            user.FullName = model.FullName;
            user.Email = model.Email;

            if (profileImageFile != null && profileImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profile");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileExt = Path.GetExtension(profileImageFile.FileName);
                var fileName = $"user_{user.Id}_{Guid.NewGuid():N}{fileExt}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImageFile.CopyToAsync(stream);
                }

                // Web'den erişilebilir path (wwwroot dışına çıkma)
                user.ProfileImagePath = "/uploads/profile/" + fileName;
            }

            _userRepository.Update(user);
            await _userRepository.SaveAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            if (!string.IsNullOrEmpty(user.ProfileImagePath))
            {
                claims.Add(new Claim("ProfileImagePath", user.ProfileImagePath));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            TempData["ProfileSuccess"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserProfileViewModel model)
        {
            // sadece şifre alanlarını validate ediyoruz
            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "New password is required.");
            }

            if (!ModelState.IsValid)
            {
                var user = await _userRepository.GetByIdAsync(model.Id);
                if (user == null)
                    return NotFound();

                model.UserName = user.UserName;
                model.FullName = user.FullName;
                model.Email = user.Email;
                model.Role = user.Role;
                model.ProfileImagePath = user.ProfileImagePath;

                return View("Profile", model);
            }

            var existingUser = await _userRepository.GetByIdAsync(model.Id);
            if (existingUser == null)
                return NotFound();

            var (hash, salt) = PasswordHelper.CreatePasswordHash(model.NewPassword!);
            existingUser.PasswordHash = hash;
            existingUser.PasswordSalt = salt;

            _userRepository.Update(existingUser);
            await _userRepository.SaveAsync();

            TempData["PasswordSuccess"] = "Password changed successfully.";
            return RedirectToAction("Profile");
        }
    }
}
