// =============================================================
// FaithTrack — ViewModels/LoginViewModel.cs
// ViewModel for the Login form.
// Matches the Login Page wireframe (Architecture Plan Fig. 8)
// and the AccountController.Login action in UML Class Diagram
// (Fig. 15, Presentation Layer).
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using System.ComponentModel.DataAnnotations;

namespace FaithTrack.ViewModels
{
    /// <summary>
    /// Carries login form data from the Login view to
    /// AccountController.Login(POST). Contains Email, Password,
    /// and RememberMe fields per the Login wireframe (Fig. 8).
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// User's registered email address. Required.
        /// Used as the username identifier in ASP.NET Core Identity.
        /// </summary>
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's account password. Required.
        /// Validated against the PBKDF2 hash stored by Identity.
        /// DataType.Password prevents the value from being displayed.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// If true, a persistent cookie is issued so the user
        /// remains logged in across browser sessions.
        /// Maps to the "Remember Me" checkbox in the Login wireframe.
        /// </summary>
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
