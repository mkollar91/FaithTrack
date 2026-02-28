// =============================================================
// FaithTrack — Controllers/HomeController.cs
// Serves the Dashboard (main landing page after login).
// Architecture Plan Fig. 9 — Dashboard Wireframe.
// Architecture Plan Fig. 7 — Sitemap (Dashboard as main hub).
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithTrack.Controllers
{
    /// <summary>
    /// Handles the Dashboard view which serves as the primary
    /// landing page for authenticated users. Provides quick
    /// access to Resources, Categories, Profile, and Logout
    /// per the Sitemap (Architecture Plan Fig. 7) and
    /// Dashboard wireframe (Fig. 9).
    /// [Authorize] ensures unauthenticated users are redirected
    /// to /Account/Login automatically by Identity middleware.
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        /// <summary>Built-in ASP.NET Core logger.</summary>
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initialises the controller with logger via DI.
        /// </summary>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// GET /Home/Index  (or just /)
        /// Returns the Dashboard view after successful authentication.
        /// This is the redirect target in AccountController.Login(POST)
        /// per the Authentication Flowchart (Architecture Plan Fig. 1).
        /// </summary>
        public IActionResult Index()
        {
            _logger.LogInformation(
                "HomeController: Dashboard accessed by {User}.",
                User.Identity?.Name);

            return View();
        }

        /// <summary>
        /// GET /Home/Error
        /// Generic error page shown in production to prevent
        /// leaking system information per Architecture Plan
        /// NFR — Operational Support Design (p.27).
        /// </summary>
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}
