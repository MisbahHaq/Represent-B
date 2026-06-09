using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using representweb.Data;
using representweb.Models;

namespace representweb.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("IsAdmin") == "true";
        }

        // GET: Admin/Login
        public IActionResult Login()
        {
            if (IsAdminLoggedIn())
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            return RedirectToAction("Login", "Account");
        }

        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(AdminLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Email == "admin@represent.com" && model.Password == "Qwerty123")
                {
                    HttpContext.Session.SetString("IsAdmin", "true");
                    HttpContext.Session.SetString("AdminEmail", model.Email);
                    return RedirectToAction("Dashboard", "Admin");
                }
                ModelState.AddModelError("", "Invalid admin credentials");
            }
            return RedirectToAction("Login", "Account");
        }

        // POST: Admin/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdmin");
            HttpContext.Session.Remove("AdminEmail");
            HttpContext.Session.Remove("UserEmail");
            HttpContext.Session.Remove("AuthToken");
            return RedirectToAction("Login", "Account");
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            var products = await _context.Products.Take(10).ToListAsync();

            var model = new AdminDashboardViewModel
            {
                TotalOrders = await _context.Orders.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending"),
                OutForDeliveryOrders = await _context.Orders.CountAsync(o => o.Status == "Out for Delivery"),
                RecentOrders = orders,
                RecentProducts = products
            };

            return View(model);
        }

        // GET: Admin/Products
        public async Task<IActionResult> Products()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // GET: Admin/Products/Create
        public IActionResult CreateProduct()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }
            return View();
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Products));
            }
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> EditProduct(int? id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Products));
            }
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> DeleteProduct(int? id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductConfirmed(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Products));
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Orders()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        // POST: Admin/Orders/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] OrderStatusUpdateModel model)
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var order = await _context.Orders.FindAsync(model.OrderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found" });
            }

            var validStatuses = new[] { "Pending", "Out for Delivery", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(model.Status))
            {
                return Json(new { success = false, message = "Invalid status" });
            }

            order.Status = model.Status;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Status updated" });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}

public class OrderStatusUpdateModel
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}