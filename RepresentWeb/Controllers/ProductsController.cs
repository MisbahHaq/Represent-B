using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepresentWeb.Data;
using RepresentWeb.Models;

namespace RepresentWeb.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string gender)
        {
            var products = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(gender))
            {
                products = products.Where(p => p.Gender == gender);
            }

            return View(await products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var related = new List<Product>();
            if (!string.IsNullOrEmpty(product.Tags))
            {
                var currentTags = product.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();

                if (currentTags.Any())
                {
                    var candidates = await _context.Products
                        .Where(p => p.Id != product.Id && p.Tags != null)
                        .ToListAsync();
                    related = candidates
                        .AsEnumerable()
                        .Where(p => p.GetTags() != null && p.GetTags()!.Any(t => currentTags.Contains(t)))
                        .ToList();
                }
            }

            ViewBag.RelatedProducts = related;
            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                product.ImageUrls = NormalizeImageUrls(product.ImageUrls, Request.Form);
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    product.ImageUrls = NormalizeImageUrls(product.ImageUrls, Request.Form);
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private static string? NormalizeImageUrls(string? existingImageUrls, IFormCollection form)
        {
            var images = new List<string>();
            if (!string.IsNullOrWhiteSpace(existingImageUrls))
            {
                images.AddRange(existingImageUrls.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(url => url.Trim()));
            }

            for (var i = 0; i < 5; i++)
            {
                var value = form[$"ImageUrls[{i}]"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    images.Add(value.Trim());
                }
            }

            return images.Any() ? string.Join(", ", images.Distinct(StringComparer.OrdinalIgnoreCase)) : existingImageUrls;
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}