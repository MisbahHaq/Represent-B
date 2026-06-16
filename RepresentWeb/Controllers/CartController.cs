using Microsoft.AspNetCore.Mvc;
using RepresentWeb.Data;
using RepresentWeb.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RepresentWeb.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cart
        public IActionResult Index()
        {
            var cartItems = GetCartItems();
            var viewModel = new CartViewModel
            {
                Items = cartItems,
                TotalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity)
            };
            return View(viewModel);
        }

        // POST: Cart/Add/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int id, int quantity = 1, string? size = null, string? color = null)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                quantity = 1;
            }

            var normalizedSize = NormalizeSize(size);
            var normalizedColor = NormalizeColor(color);

            // Add to cart (stored in session and database if logged in)
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == id && item.Size == normalizedSize && item.Color == normalizedColor);

            // Also save to database if user is logged in
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(userEmail))
            {
                var dbCartItem = normalizedSize == string.Empty && normalizedColor == string.Empty
                    ? _context.ShoppingCarts.FirstOrDefault(c => c.UserEmail == userEmail && c.ProductId == id && (c.Size == null || c.Size == normalizedSize) && (c.Color == null || c.Color == normalizedColor))
                    : _context.ShoppingCarts.FirstOrDefault(c => c.UserEmail == userEmail && c.ProductId == id && c.Size == normalizedSize && c.Color == normalizedColor);
                if (dbCartItem != null)
                {
                    dbCartItem.Quantity += quantity;
                }
                else
                {
                    _context.ShoppingCarts.Add(new ShoppingCart { UserEmail = userEmail, ProductId = id, Quantity = quantity, Size = normalizedSize, Color = normalizedColor });
                }
                _context.SaveChanges();
            }

            if (cartItem != null)
            {
                // Update quantity if item already exists
                cartItem.Quantity += quantity;
            }
            else
            {
                // Add new item
                cart.Add(new CartItem { ProductId = id, Quantity = quantity, Size = normalizedSize, Color = normalizedColor });
            }

            SaveCart(cart);

            // Return JSON for AJAX requests or redirect for form posts
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, cartCount = cart.Sum(item => item.Quantity) });
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        // POST: Cart/Remove/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int id, string? size = null, string? color = null)
        {
            var normalizedSize = NormalizeSize(size);
            var normalizedColor = NormalizeColor(color);
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == id && item.Size == normalizedSize && item.Color == normalizedColor);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCart(cart);

                // Also remove from database if logged in
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var dbCartItem = normalizedSize == string.Empty && normalizedColor == string.Empty
                    ? _context.ShoppingCarts.FirstOrDefault(c => c.UserEmail == userEmail && c.ProductId == id && (c.Size == null || c.Size == normalizedSize) && (c.Color == null || c.Color == normalizedColor))
                    : _context.ShoppingCarts.FirstOrDefault(c => c.UserEmail == userEmail && c.ProductId == id && c.Size == normalizedSize && c.Color == normalizedColor);
                    if (dbCartItem != null)
                    {
                        _context.ShoppingCarts.Remove(dbCartItem);
                        _context.SaveChanges();
                    }
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, cartCount = cart.Sum(item => item.Quantity) });
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        // POST: Cart/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int id, int quantity, string? size = null, string? color = null)
        {
            if (quantity <= 0)
            {
                return Remove(id, size, color);
            }

            var normalizedSize = NormalizeSize(size);
            var normalizedColor = NormalizeColor(color);
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == id && item.Size == normalizedSize && item.Color == normalizedColor);

            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                SaveCart(cart);

                // Also update in database if logged in
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var dbCartItem = normalizedSize == string.Empty && normalizedColor == string.Empty
                    ? _context.ShoppingCarts.FirstOrDefault(c => c.UserEmail == userEmail && c.ProductId == id && (c.Size == null || c.Size == normalizedSize) && (c.Color == null || c.Color == normalizedColor))
                    : _context.ShoppingCarts.FirstOrDefault(c => c.UserEmail == userEmail && c.ProductId == id && c.Size == normalizedSize && c.Color == normalizedColor);
                    if (dbCartItem != null)
                    {
                        dbCartItem.Quantity = quantity;
                        _context.SaveChanges();
                    }
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        // GET: Cart/GetCount
        [HttpGet]
        public IActionResult GetCount()
        {
            var cart = GetCart();
            var count = cart.Sum(item => item.Quantity);
            return Json(new { count = count });
        }

        // GET: Cart/Checkout
        [HttpGet]
        public IActionResult Checkout()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var fullName = HttpContext.Session.GetString("UserName") ?? string.Empty;
            var nameParts = fullName.Split(' ', 2);
            var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            var cartItems = GetCartItems();
            if (!cartItems.Any())
            {
                return RedirectToAction("Index");
            }

            var viewModel = new CartViewModel
            {
                Items = cartItems,
                TotalAmount = cartItems.Sum(item => item.Product?.Price * item.Quantity ?? 0),
                FirstName = firstName,
                LastName = lastName,
                Email = userEmail,
                Address = HttpContext.Session.GetString("UserAddress") ?? string.Empty,
                Country = string.Empty,
                City = string.Empty
            };

            return View(viewModel);
        }

        // POST: Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CartViewModel model, string deliveryMethod, string paymentMethod)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                model.Items = GetCartItems();
                model.TotalAmount = model.Items.Sum(item => item.Product?.Price * item.Quantity ?? 0);
                return View(model);
            }

            var cartItems = GetCartItems();
            if (!cartItems.Any())
            {
                return RedirectToAction("Index");
            }

            var subtotal = cartItems.Sum(item => item.Product.Price * item.Quantity);
            var deliveryFee = 0m;
            if (deliveryMethod == "express")
            {
                deliveryFee = 500m;
            }

            var paymentFee = 0m;
            if (paymentMethod == "cod")
            {
                paymentFee = 800m;
            }

            // Create order
            var order = new Order
            {
                UserEmail = userEmail,
                Address = model.Address,
                Country = model.Country,
                City = model.City,
                PhoneNumber = model.Phone,
                OrderDate = DateTime.Now,
                Status = "Pending",
                DeliveryMethod = deliveryMethod,
                PaymentMethod = paymentMethod,
                TotalAmount = subtotal + deliveryFee + paymentFee,
                Items = new List<OrderItem>()
            };

            foreach (var cartItem in cartItems)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = cartItem.Product.Id,
                    ProductName = cartItem.Product.Name,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price,
                    ImageUrl = cartItem.Product.ImageUrl,
                    Size = cartItem.Size,
                    Color = cartItem.Color
                });
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.SetString("UserAddress", model.Address);
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user != null)
            {
                user.Address = model.Address;
                _context.SaveChanges();
            }

            // Clear cart from both session and database
            var cart = GetCart();
            cart.Clear();
            SaveCart(cart);

            if (!string.IsNullOrEmpty(userEmail))
            {
                var dbCartItems = _context.ShoppingCarts.Where(c => c.UserEmail == userEmail);
                _context.ShoppingCarts.RemoveRange(dbCartItems);
                _context.SaveChanges();
            }

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        // GET: Cart/OrderConfirmation/5
        [HttpGet]
        public IActionResult OrderConfirmation(int id)
        {
            var order = _context.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            // Ensure the order belongs to the current user (optional)
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (order.UserEmail != userEmail)
            {
                return Unauthorized();
            }

            return View(order);
        }

        private List<CartItem> GetCart()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            // If user is logged in, try to merge database cart into session
            if (!string.IsNullOrEmpty(userEmail))
            {
                var dbCartItems = _context.ShoppingCarts.Where(c => c.UserEmail == userEmail).ToList();

                // Only merge if session is empty
                var sessionCartJson = HttpContext.Session.GetString("Cart");
                if (string.IsNullOrEmpty(sessionCartJson))
                {
                    var mergedCart = dbCartItems.Select(c => new CartItem
                    {
                        ProductId = c.ProductId,
                        Quantity = c.Quantity,
                        Size = c.Size ?? string.Empty,
                        Color = c.Color ?? string.Empty
                    }).ToList();

                    var cartJsonToSave = System.Text.Json.JsonSerializer.Serialize(mergedCart);
                    HttpContext.Session.SetString("Cart", cartJsonToSave);
                    return mergedCart;
                }
            }

            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }

            return System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var cartJson = System.Text.Json.JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }

        private static string NormalizeSize(string? size)
        {
            return (size ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static string NormalizeColor(string? color)
        {
            return (color ?? string.Empty).Trim();
        }

        private List<CartViewModel.CartItemViewModel> GetCartItems()
        {
            var cart = GetCart();
            var productIds = cart.Select(item => item.ProductId).ToList();
            var products = _context.Products.Where(p => productIds.Contains(p.Id)).ToList();
            
            var cartItems = new List<CartViewModel.CartItemViewModel>();
            foreach (var cartItem in cart)
            {
                var product = products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                if (product != null)
                {
                    cartItems.Add(new CartViewModel.CartItemViewModel
                    {
                        Product = product,
                        Quantity = cartItem.Quantity,
                        Size = cartItem.Size ?? string.Empty,
                        Color = cartItem.Color ?? string.Empty
                    });
                }
            }

            return cartItems;
        }

        public class CartViewModel
        {
            public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
            public decimal TotalAmount { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            [Required(ErrorMessage = "Address is required.")]
            public string Address { get; set; } = string.Empty;
            [Required(ErrorMessage = "Country / region is required.")]
            public string Country { get; set; } = string.Empty;
            [Required(ErrorMessage = "City is required.")]
            public string City { get; set; } = string.Empty;
            [Required(ErrorMessage = "Phone number is required.")]
            public string Phone { get; set; } = string.Empty;

            public class CartItemViewModel
            {
                public Product? Product { get; set; }
                public int Quantity { get; set; }
                public string Size { get; set; } = string.Empty;
                public string Color { get; set; } = string.Empty;
                public decimal Subtotal => Product?.Price * Quantity ?? 0;
            }
        }
    }
}