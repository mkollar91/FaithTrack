// =============================================================
// FaithTrack — ViewModels/RegisterViewModel.cs
// ViewModel for the user registration form (Sprint 2, US-14).
// Included as a draft stub in Sprint 1 so the AccountController
// compiles fully. Full view implementation is Sprint 2.
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using System.ComponentModel.DataAnnotations;

namespace FaithTrack.ViewModels
{
    /// <summary>
    /// Carries registration form data from the Register view
    /// to AccountController.Register(POST).
    /// Sprint 2 — User Story 14: Register for a new account.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// New user's email address. Must be unique in the system.
        /// Used as the login username via ASP.NET Core Identity.
        /// </summary>
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Desired password. Must meet complexity requirements
        /// configured in Program.cs per NFR — Password Security (p.26):
        /// min 8 chars, uppercase, lowercase, digit, special character.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation field to ensure the user typed their
        /// desired password correctly. Must match Password.
        /// </summary>
        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
