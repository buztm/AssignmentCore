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
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepository;

        public IActionResult ViewProfile()
        {
            return View();
        }

        public AccountController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["title"] = "Sign In";
            ViewData["subTitle"] = "Authentication";

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var allUsers = await _userRepository.GetAllAsync();
            var user = allUsers
                .FirstOrDefault(u =>
                    (u.UserName == model.UserNameOrEmail || u.Email == model.UserNameOrEmail) &&
                    u.IsActive);

            if (user == null ||
                !PasswordHelper.VerifyPassword(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FullName", user.FullName)
            };

            if (!string.IsNullOrEmpty(user.ProfileImagePath))
            {
                claims.Add(new Claim("ProfileImagePath", user.ProfileImagePath));
            }

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties();

            if (model.RememberMe)
            {
                authProperties.IsPersistent = true;
                authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7); // örnek: 7 gün
            }

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}