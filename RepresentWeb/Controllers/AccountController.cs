using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using RepresentWeb.Models;
using RepresentWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace RepresentWeb.Controllers
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
                // Check admin credentials first (hardcoded special case)
                if (model.Email == "admin@represent.com" && model.Password == "Qwerty123")
                {
                    HttpContext.Session.SetString("UserEmail", model.Email);
                    HttpContext.Session.SetString("AuthToken", "fake-jwt-token");
                    HttpContext.Session.SetString("UserName", "Admin");
                    HttpContext.Session.SetString("UserAddress", string.Empty);
                    HttpContext.Session.SetString("IsAdmin", "true");
                    return RedirectToAction("Dashboard", "Admin");
                }
                
                // Check regular user in database
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
                
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(model);
                }

                // Restore cart from database to session
                var dbCartItems = _context.ShoppingCarts.Where(c => c.UserEmail == model.Email).ToList();
                var sessionCart = dbCartItems.Select(c => new RepresentWeb.Models.CartItem { ProductId = c.ProductId, Quantity = c.Quantity, Size = c.Size ?? string.Empty, Color = c.Color ?? string.Empty }).ToList();
                var cartJson = System.Text.Json.JsonSerializer.Serialize(sessionCart);
                HttpContext.Session.SetString("Cart", cartJson);

                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("AuthToken", "fake-jwt-token");
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserAddress", user.Address);
                
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
                // Check admin credentials first (hardcoded special case)
                if (model.Email == "admin@represent.com" && model.Password == "Qwerty123")
                {
                    HttpContext.Session.SetString("UserEmail", model.Email);
                    HttpContext.Session.SetString("AuthToken", "fake-jwt-token");
                    HttpContext.Session.SetString("UserName", "Admin");
                    HttpContext.Session.SetString("UserAddress", string.Empty);
                    HttpContext.Session.SetString("IsAdmin", "true");
                    return Json(new { success = true });
                }
                
                // Check regular user in database
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

                if (user == null)
                {
                    return Json(new { success = false, errors = new[] { "Invalid email or password." } });
                }

                // Restore cart from database to session
                var dbCartItems = _context.ShoppingCarts.Where(c => c.UserEmail == model.Email).ToList();
                var sessionCart = dbCartItems.Select(c => new RepresentWeb.Models.CartItem { ProductId = c.ProductId, Quantity = c.Quantity, Size = c.Size ?? string.Empty, Color = c.Color ?? string.Empty }).ToList();
                var cartJson = System.Text.Json.JsonSerializer.Serialize(sessionCart);
                HttpContext.Session.SetString("Cart", cartJson);

                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("AuthToken", "fake-jwt-token");
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserAddress", user.Address);
                
                if (model.RememberMe)
                {
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
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "An account with this email already exists.");
                    return View(model);
                }
                
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    DateOfBirth = model.DateOfBirth,
                    Address = model.Address,
                    Password = model.Password
                };
                
                _context.Users.Add(user);
                _context.SaveChanges();
                
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
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    return Json(new { success = false, errors = new[] { "An account with this email already exists." } });
                }
                
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    DateOfBirth = model.DateOfBirth,
                    Address = model.Address,
                    Password = model.Password
                };
                
                _context.Users.Add(user);
                _context.SaveChanges();
                
                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("AuthToken", "fake-jwt-token");
                HttpContext.Session.SetString("UserName", user.Name);

                return Json(new { success = true });
            }
            
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            // Save cart to database before clearing session
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(userEmail))
            {
                var cartJson = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    var cart = System.Text.Json.JsonSerializer.Deserialize<List<RepresentWeb.Models.CartItem>>(cartJson) ?? new List<RepresentWeb.Models.CartItem>();

                    // Sync session cart to database
                    foreach (var item in cart)
                    {
                        var dbItem = item.Size == string.Empty && item.Color == string.Empty
                            ? _context.ShoppingCarts.FirstOrDefault(c => c.UserEmail == userEmail && c.ProductId == item.ProductId && (c.Size == null || c.Size == item.Size) && (c.Color == null || c.Color == item.Color))
                            : _context.ShoppingCarts.FirstOrDefault(c => c.UserEmail == userEmail && c.ProductId == item.ProductId && c.Size == item.Size && c.Color == item.Color);
                        if (dbItem != null)
                        {
                            dbItem.Quantity = item.Quantity;
                        }
                        else
                        {
                            _context.ShoppingCarts.Add(new RepresentWeb.Models.ShoppingCart
                            {
                                UserEmail = userEmail,
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                Size = item.Size,
                                Color = item.Color,
                                CreatedAt = DateTime.Now
                            });
                        }
                    }
                    _context.SaveChanges();
                }
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // POST: Account/LogoutAjax
        [HttpPost]
        public IActionResult LogoutAjax()
        {
            // Save cart to database before clearing session
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(userEmail))
            {
                var cartJson = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    var cart = System.Text.Json.JsonSerializer.Deserialize<List<RepresentWeb.Models.CartItem>>(cartJson) ?? new List<RepresentWeb.Models.CartItem>();

                    // Sync session cart to database
                    foreach (var item in cart)
                    {
                        var dbItem = item.Size == string.Empty && item.Color == string.Empty
                            ? _context.ShoppingCarts.FirstOrDefault(c => c.UserEmail == userEmail && c.ProductId == item.ProductId && (c.Size == null || c.Size == item.Size) && (c.Color == null || c.Color == item.Color))
                            : _context.ShoppingCarts.FirstOrDefault(c => c.UserEmail == userEmail && c.ProductId == item.ProductId && c.Size == item.Size && c.Color == item.Color);
                        if (dbItem != null)
                        {
                            dbItem.Quantity = item.Quantity;
                        }
                        else
                        {
                            _context.ShoppingCarts.Add(new RepresentWeb.Models.ShoppingCart
                            {
                                UserEmail = userEmail,
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                Size = item.Size,
                                Color = item.Color,
                                CreatedAt = DateTime.Now
                            });
                        }
                    }
                    _context.SaveChanges();
                }
            }

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
            return Json(new { authenticated = true, userEmail = userEmail, userName = HttpContext.Session.GetString("UserName") ?? string.Empty, isAdmin = isAdmin });
        }

        // GET: Account/Profile
        public IActionResult Profile()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }
            if (HttpContext.Session.GetString("IsAdmin") == "true")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            ViewData["Title"] = "My Account";

            var model = new ProfileViewModel
            {
                UserEmail = userEmail,
                UserName = HttpContext.Session.GetString("UserName") ?? string.Empty,
                UserAddress = HttpContext.Session.GetString("UserAddress") ?? string.Empty,
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
            var model = new UpdateUsernameViewModel { Username = username ?? string.Empty };
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
            var model = new UpdateAddressViewModel { Address = address ?? string.Empty };
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
            var products = _context.Products.Take(4).ToList();
            if (products.Any())
            {
                return products;
            }

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
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class SignUpViewModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UpdateUsernameViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;
    }

    public class UpdateAddressViewModel
    {
        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;
    }
}