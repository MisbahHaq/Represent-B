using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

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
                // For demo, we'll accept any email/password combination
                // Set session or cookie to indicate logged in state
                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("AuthToken", "fake-jwt-token");
                
                if (model.RememberMe)
                {
                    // Set longer expiration for remember me
                    // This would be handled by cookie options in real app
                }
                
                return RedirectToAction("Index", "Home");
            }
            
            return View(model);
        }

        // POST: Account/LoginAjax
        [HttpPost]
        public IActionResult LoginAjax([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // In a real app, validate credentials against database
                // For demo, we'll accept any email/password combination
                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("AuthToken", "fake-jwt-token");
                
                return Json(new { success = true });
            }
            
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
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

        // POST: Account/SignUpAjax
        [HttpPost]
        public IActionResult SignUpAjax([FromBody] SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                // In a real app, create user in database and log them in
                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("AuthToken", "fake-jwt-token");
                
                return Json(new { success = true });
            }
            
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            // Clear session
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // POST: Account/LogoutAjax
        [HttpPost]
        public IActionResult LogoutAjax()
        {
            HttpContext.Session.Clear();
            return Json(new { success = true });
        }

        // GET: Account/CheckAuth
        [HttpGet]
        public IActionResult CheckAuth()
        {
            var isAuthenticated = !string.IsNullOrEmpty(HttpContext.Session.GetString("AuthToken"));
            return Json(new { authenticated = isAuthenticated, userEmail = HttpContext.Session.GetString("UserEmail") });
        }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

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