using Microsoft.AspNetCore.Mvc;
using representweb.Data;
using representweb.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace representweb.Controllers
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
        public IActionResult Add(int id, int quantity = 1)
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

            // Add to cart (stored in session)
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == id);

            if (cartItem != null)
            {
                // Update quantity if item already exists
                cartItem.Quantity += quantity;
            }
            else
            {
                // Add new item
                cart.Add(new CartItem { ProductId = id, Quantity = quantity });
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
        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == id);
            
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCart(cart);
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
        public IActionResult Update(int id, int quantity)
        {
            if (quantity <= 0)
            {
                return Remove(id);
            }

            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == id);
            
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                SaveCart(cart);
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

            var address = HttpContext.Session.GetString("UserAddress");
            if (string.IsNullOrEmpty(address))
            {
                // Redirect to update address
                return RedirectToAction("UpdateAddress", "Account");
            }

            var cartItems = GetCartItems();
            if (!cartItems.Any())
            {
                return RedirectToAction("Index");
            }

            var viewModel = new CartViewModel
            {
                Items = cartItems,
                TotalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity)
            };

            return View(viewModel);
        }

        // POST: Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CartViewModel model)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var address = HttpContext.Session.GetString("UserAddress");
            if (string.IsNullOrEmpty(address))
            {
                return RedirectToAction("UpdateAddress", "Account");
            }

            var cartItems = GetCartItems();
            if (!cartItems.Any())
            {
                return RedirectToAction("Index");
            }

            // Create order
            var order = new Order
            {
                UserEmail = userEmail,
                OrderDate = DateTime.Now,
                Status = "ToShip",
                TotalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity),
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
                    ImageUrl = cartItem.Product.ImageUrl
                });
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Clear cart
            var cart = GetCart();
            cart.Clear();
            SaveCart(cart);

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
                        Quantity = cartItem.Quantity
                    });
                }
            }
            
            return cartItems;
        }
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal TotalAmount { get; set; }

        public class CartItemViewModel
        {
            public Product Product { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal => Product.Price * Quantity;
        }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}