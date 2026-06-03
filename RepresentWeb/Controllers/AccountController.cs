using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace representweb.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // In a real app, validate credentials against database
                // For now, we'll just set a token in localStorage via JavaScript
                // This is a simplified implementation
                return RedirectToAction("Index", "Home");
            }
            
            return View(model);
        }

        // GET: Account/SignUp
        public IActionResult SignUp()
        {
            return View();
        }

        // POST: Account/SignUp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                // In a real app, create user in database
                // For now, just redirect to login
                return RedirectToAction("Login");
            }
            
            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            // In a real app, clear authentication
            return RedirectToAction("Index", "Home");
        }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class SignUpViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}