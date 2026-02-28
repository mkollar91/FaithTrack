// =============================================================
// FaithTrack — Controllers/AccountController.cs
// Handles user authentication: Login, Logout, and Register.
// Defined in Architecture Plan UML Class Diagram (Fig. 15),
// Presentation Layer — AccountController.
// Authentication flow matches Architecture Plan Fig. 1
// (Authentication Process Flowchart).
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using FaithTrack.Models;
using FaithTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FaithTrack.Controllers
{
    /// <summary>
    /// Manages user authentication for the FaithTrack application.
    /// Uses ASP.NET Core Identity SignInManager and UserManager as
    /// specified in the Architecture Plan NFR — Authentication (p.26).
    /// Login/Logout flow implements the Authentication Flowchart (Fig. 1).
    /// </summary>
    public class AccountController : Controller
    {
        // ── Private Fields ───────────────────────────────────

        /// <summary>Identity service for signing users in and out.</summary>
        private readonly SignInManager<ApplicationUser> _signInManager;

        /// <summary>Identity service for creating and managing users.</summary>
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>Built-in ASP.NET Core logger.</summary>
        private readonly ILogger<AccountController> _logger;

        // ── Constructor ──────────────────────────────────────

        /// <summary>
        /// Initialises the controller with Identity services and
        /// logger provided by dependency injection.
        /// </summary>
        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager   = userManager;
            _logger        = logger;
        }

        // ── Login ────────────────────────────────────────────

        /// <summary>
        /// GET /Account/Login
        /// Returns the Login view (Architecture Plan Fig. 8 wireframe).
        /// Redirects authenticated users directly to the Dashboard.
        /// </summary>
        /// <param name="returnUrl">URL to redirect to after login.</param>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            // If already authenticated, skip login page
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// POST /Account/Login
        /// Validates credentials via ASP.NET Core Identity and
        /// redirects to Dashboard on success, or returns the Login
        /// view with an error message on failure.
        /// Implements the Authentication Flowchart (Fig. 1):
        ///   Valid → Create Session → Redirect to Dashboard
        ///   Invalid → Display Error Message
        /// [ValidateAntiForgeryToken] prevents CSRF attacks per
        /// Architecture Plan NFR — Security (p.26).
        /// </summary>
        /// <param name="model">Login form data (email, password, rememberMe).</param>
        /// <param name="returnUrl">URL to redirect to after login.</param>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // ModelState checks [Required] and [EmailAddress] annotations
            if (!ModelState.IsValid)
                return View(model);

            _logger.LogInformation(
                "AccountController: Login attempt for {Email}.", model.Email);

            // Attempt password sign-in via Identity
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "AccountController: Login successful for {Email}.", model.Email);

                // Redirect to the originally requested URL or Dashboard
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            // Authentication failed — display error message per Fig. 1
            _logger.LogWarning(
                "AccountController: Login failed for {Email}.", model.Email);

            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
            return View(model);
        }

        // ── Logout ───────────────────────────────────────────

        /// <summary>
        /// POST /Account/Logout
        /// Signs the user out and clears the session cookie.
        /// Redirects to the Login page.
        /// POST is used (not GET) to prevent logout via link
        /// click (CSRF protection per NFR Security, p.26).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation(
                "AccountController: User {Name} logging out.", User.Identity?.Name);

            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // ── Register (Sprint 2 — US-14) ──────────────────────

        /// <summary>
        /// GET /Account/Register
        /// Returns the Registration view. Sprint 2 — US-14.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// POST /Account/Register
        /// Creates a new user account using ASP.NET Core Identity
        /// UserManager. Redirects to Login on success.
        /// Sprint 2 — US-14: Register for a new account.
        /// </summary>
        /// <param name="model">Registration form data.</param>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _logger.LogInformation(
                "AccountController: Register attempt for {Email}.", model.Email);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "AccountController: Account created for {Email}.", model.Email);
                return RedirectToAction("Login", "Account");
            }

            // Add Identity errors to ModelState for display in the view
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // ── Access Denied ────────────────────────────────────

        /// <summary>
        /// GET /Account/AccessDenied
        /// Shown when an authenticated user attempts to access
        /// a resource they do not have permission for.
        /// </summary>
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ── Private Helpers ──────────────────────────────────

        /// <summary>
        /// Returns the authenticated user's ASP.NET Identity ID
        /// string from the current ClaimsPrincipal.
        /// </summary>
        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User) ?? string.Empty;
        }
    }
}
