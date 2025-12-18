using AspNetCoreHero.ToastNotification.Abstractions;
using AssignmentCore.Hubs;
using AssignmentCore.Models;
using AssignmentCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AssignmentCore.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IWebHostEnvironment _env;
        private readonly INotyfService _notyf;
        private readonly IHubContext<GeneralHub> _hubContext;

        public UserController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IWebHostEnvironment env, INotyfService notyf, IHubContext<GeneralHub> hubContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
            _notyf = notyf;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Teachers()
        {
            ViewData["title"] = "Teachers";
            ViewData["subTitle"] = "Teacher List";

            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            return View(teachers);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Students()
        {
            ViewData["title"] = "Students";
            ViewData["subTitle"] = "Student List";

            var students = await _userManager.GetUsersInRoleAsync("Student");
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

            var user = new User
            {
                UserName = model.UserName,
                FullName = model.FullName,
                Email = model.Email,
                IsActive = model.IsActive,
                Role = model.Role
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            if (!string.IsNullOrEmpty(model.Role))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return View(model);
                }
            }

            _notyf.Success("User created successfully"); 

            if (model.Role == "Teacher")
                return RedirectToAction("Teachers");
            else
                return RedirectToAction("Students");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            ViewData["title"] = "Edit User";
            ViewData["subTitle"] = user.Role == "Teacher" ? "Update Teacher" : "Update Student";

            var vm = new UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                FullName = user.FullName,
                Email = user.Email!,
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

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            user.UserName = model.UserName;
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.IsActive = model.IsActive;
            user.Role = model.Role;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var pwdResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (!pwdResult.Succeeded)
                {
                    foreach (var error in pwdResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return View(model);
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);
            if (!string.IsNullOrEmpty(model.Role))
                await _userManager.AddToRoleAsync(user, model.Role);

            _notyf.Information("User updated successfully");

            if (model.Role == "Teacher")
                return RedirectToAction("Teachers");
            else
                return RedirectToAction("Students");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            ViewData["title"] = "Delete User";
            ViewData["subTitle"] = user.Role == "Teacher" ? "Delete Teacher" : "Delete Student";

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            var role = user.Role;
            await _userManager.DeleteAsync(user);

            _notyf.Warning("User deleted successfully");

            if (role == "Teacher")
                return RedirectToAction("Teachers");
            else
                return RedirectToAction("Students");
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string? tab = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "-";

            var vm = new UserProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                FullName = user.FullName,
                Email = user.Email!,
                Role = role,
                ProfileImagePath = user.ProfileImagePath
            };

            ViewBag.ActiveTab = tab;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(
            UserProfileViewModel model,
            IFormFile? profileImageFile)
        {
            if (!ModelState.IsValid)
                return View("Profile", model);

            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null)
                return NotFound();

            user.UserName = model.UserName;
            user.FullName = model.FullName;
            user.Email = model.Email;

            if (profileImageFile != null && profileImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profile");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileExt = Path.GetExtension(profileImageFile.FileName);
                var fileName = $"user_{user.Id}_{Guid.NewGuid():N}{fileExt}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImageFile.CopyToAsync(stream);
                }

                user.ProfileImagePath = "/uploads/profile/" + fileName;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View("Profile", model);
            }

            await _signInManager.RefreshSignInAsync(user);

            _notyf.Information("Profile updated successfully");

            TempData["ProfileSuccess"] = "Profile updated successfully.";
            return RedirectToAction("Profile", new { tab = "edit" });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserProfileViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "New password is required.");
                return await Profile();
            }

            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null)
                return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword!);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                model.UserName = user.UserName!;
                model.FullName = user.FullName;
                model.Email = user.Email!;
                model.Role = user.Role;
                model.ProfileImagePath = user.ProfileImagePath;

                return View("Profile", model);
            }

            _notyf.Information("Password changed successfully");

            TempData["PasswordSuccess"] = "Password changed successfully.";
            return RedirectToAction("Profile", new { tab = "password" });
        }
    }
}