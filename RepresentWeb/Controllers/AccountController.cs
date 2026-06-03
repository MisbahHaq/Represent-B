using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Linq;
using representweb.Models;
using representweb.Data;

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
                Orders = GetMockOrders(userEmail),
                RecentlyViewed = GetMockRecentlyViewedProducts()
            };

            return View(model);
        }

        // GET: Account/CheckAuth
        [HttpGet]
        public IActionResult CheckAuth()
        {
            var isAuthenticated = !string.IsNullOrEmpty(HttpContext.Session.GetString("AuthToken"));
            return Json(new { authenticated = isAuthenticated, userEmail = HttpContext.Session.GetString("UserEmail") });
        }

        private List<Order> GetMockOrders(string userEmail)
        {
            // Create some mock orders with different statuses
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    UserEmail = userEmail,
                    OrderDate = DateTime.Now.AddDays(-10),
                    Status = "ToShip",
                    TotalAmount = 89.99m,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Id = 1, OrderId = 1, ProductId = 1, ProductName = "Represent T-Shirt", Quantity = 2, Price = 29.99m, ImageUrl = "/images/tshirt1.jpg" },
                        new OrderItem { Id = 2, OrderId = 1, ProductId = 2, ProductName = "Represent Hoodie", Quantity = 1, Price = 39.99m, ImageUrl = "/images/hoodie1.jpg" }
                    }
                },
                new Order
                {
                    Id = 2,
                    UserEmail = userEmail,
                    OrderDate = DateTime.Now.AddDays(-5),
                    Status = "ToReview",
                    TotalAmount = 49.99m,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Id = 3, OrderId = 2, ProductId = 3, ProductName = "Represent Cap", Quantity = 1, Price = 49.99m, ImageUrl = "/images/cap1.jpg" }
                    }
                },
                new Order
                {
                    Id = 3,
                    UserEmail = userEmail,
                    OrderDate = DateTime.Now.AddDays(-2),
                    Status = "Returned",
                    TotalAmount = 0m,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Id = 4, OrderId = 3, ProductId = 4, ProductName = "Represent Jeans", Quantity = 1, Price = 79.99m, ImageUrl = "/images/jeans1.jpg" }
                    }
                },
                new Order
                {
                    Id = 4,
                    UserEmail = userEmail,
                    OrderDate = DateTime.Now.AddDays(-1),
                    Status = "Cancelled",
                    TotalAmount = 0m,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Id = 5, OrderId = 4, ProductId = 5, ProductName = "Represent Sneakers", Quantity = 1, Price = 89.99m, ImageUrl = "/images/sneakers1.jpg" }
                    }
                }
            };

            return orders;
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
}