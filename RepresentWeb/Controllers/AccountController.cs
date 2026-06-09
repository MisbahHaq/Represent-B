using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Linq;
using representweb.Models;
using representweb.Data;
using Microsoft.EntityFrameworkCore;

namespace representweb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

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
                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("AuthToken", "fake-jwt-token");
                var username = model.Email.Split('@')[0];
                HttpContext.Session.SetString("UserName", username);
                HttpContext.Session.SetString("UserAddress", string.Empty);
                
                if (model.Email == "admin@represent.com" && model.Password == "admin123")
                {
                    HttpContext.Session.SetString("IsAdmin", "true");
                    return RedirectToAction("Dashboard", "Admin");
                }
                
                if (model.RememberMe)
                {
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
                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("AuthToken", "fake-jwt-token");
                
                if (model.Email == "admin@represent.com" && model.Password == "admin123")
                {
                    HttpContext.Session.SetString("IsAdmin", "true");
                }
                
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
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Json(new { authenticated = false });
            }
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
            return Json(new { authenticated = true, userEmail = userEmail, isAdmin = isAdmin });
        }

        // GET: Account/Profile
        public IActionResult Profile()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }
            ViewData["Title"] = "My Account";

            var model = new ProfileViewModel
            {
                UserEmail = userEmail,
                UserName = HttpContext.Session.GetString("UserName"),
                UserAddress = HttpContext.Session.GetString("UserAddress"),
                Orders = GetUserOrders(userEmail),
                RecentlyViewed = GetMockRecentlyViewedProducts()
            };

            return View(model);
        }

        // GET: Account/UpdateUsername
        [HttpGet]
        public IActionResult UpdateUsername()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }
            var username = HttpContext.Session.GetString("UserName");
            var model = new UpdateUsernameViewModel { Username = username };
            return View(model);
        }

        // POST: Account/UpdateUsername
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateUsername(UpdateUsernameViewModel model)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }
            if (ModelState.IsValid)
            {
                HttpContext.Session.SetString("UserName", model.Username);
                return RedirectToAction("Profile");
            }
            return View(model);
        }

        // GET: Account/UpdateAddress
        [HttpGet]
        public IActionResult UpdateAddress()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }
            var address = HttpContext.Session.GetString("UserAddress");
            var model = new UpdateAddressViewModel { Address = address };
            return View(model);
        }

        // POST: Account/UpdateAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAddress(UpdateAddressViewModel model)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }
            if (ModelState.IsValid)
            {
                HttpContext.Session.SetString("UserAddress", model.Address);
                return RedirectToAction("Profile");
            }
            return View(model);
        }

        private List<Order> GetUserOrders(string userEmail)
        {
            return _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserEmail == userEmail)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        private List<Product> GetMockRecentlyViewedProducts()
        {
            // Get some products from the database or create mock products
            var products = _context.Products.Take(4).ToList();
            if (products.Any())
            {
                return products;
            }

            // If no products in DB, create mock products
            return new List<Product>
            {
                new Product { Id = 1, Name = "Represent T-Shirt", Price = 29.99m, ImageUrl = "/images/tshirt1.jpg" },
                new Product { Id = 2, Name = "Represent Hoodie", Price = 39.99m, ImageUrl = "/images/hoodie1.jpg" },
                new Product { Id = 3, Name = "Represent Cap", Price = 49.99m, ImageUrl = "/images/cap1.jpg" },
                new Product { Id = 4, Name = "Represent Jeans", Price = 79.99m, ImageUrl = "/images/jeans1.jpg" }
            };
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

    public class UpdateUsernameViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }
    }

    public class UpdateAddressViewModel
    {
        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }
    }
}