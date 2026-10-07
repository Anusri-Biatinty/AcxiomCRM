using AcxiomCRM.Models.Identity;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser>  _users;
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly IAuditService                 _audit;

        public AccountController(
            UserManager<ApplicationUser>  users,
            SignInManager<ApplicationUser> signIn,
            IAuditService audit)
        {
            _users  = users;
            _signIn = signIn;
            _audit  = audit;
        }

        // GET /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var ip   = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var user = await _users.FindByEmailAsync(model.Email);

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials.");
                await _audit.LogAsync("anonymous", model.Email, "FailedLogin",
                    "Authentication", result: "Failed", ipAddress: ip);
                return View(model);
            }

            var result = await _signIn.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await _audit.LogAsync(user.Id, user.Email ?? "", "Login",
                    "Authentication", user.Id, ipAddress: ip);
                return LocalRedirect(returnUrl ?? "/Dashboard/Index");
            }

            if (result.IsLockedOut)
            {
                await _audit.LogAsync(user.Id, user.Email ?? "", "AccountLocked",
                    "Authentication", user.Id, result: "Locked", ipAddress: ip);
                ModelState.AddModelError(string.Empty,
                    "Account locked due to too many failed attempts. Try again in 15 minutes.");
                return View(model);
            }

            await _audit.LogAsync(user.Id, user.Email ?? "", "FailedLogin",
                "Authentication", user.Id, result: "Failed", ipAddress: ip);
            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            return View(model);
        }

        // GET /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var ip   = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var user = new ApplicationUser
            {
                UserName       = model.Email,
                Email          = model.Email,
                FullName       = model.FullName,
                IsActive       = true,
                EmailConfirmed = true
            };

            var result = await _users.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _users.AddToRoleAsync(user, model.Role);
                await _audit.LogAsync(user.Id, user.Email ?? "", "Register",
                    "Authentication", user.Id, ipAddress: ip);
                await _signIn.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        // POST /Account/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var user = await _users.GetUserAsync(User);
            var ip   = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (user != null)
                await _audit.LogAsync(user.Id, user.Email ?? "", "Logout",
                    "Authentication", user.Id, ipAddress: ip);
            await _signIn.SignOutAsync();
            return RedirectToAction("Login");
        }

        // GET /Account/AccessDenied
        public IActionResult AccessDenied() => View();
    }
}
