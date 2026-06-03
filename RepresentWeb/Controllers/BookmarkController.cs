using Microsoft.AspNetCore.Mvc;
using representweb.Data;
using representweb.Models;
using System.Collections.Generic;
using System.Linq;

namespace representweb.Controllers
{
    public class BookmarkController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookmarkController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookmark
        public IActionResult Index()
        {
            var bookmarkedItems = GetBookmarkedItems();
            var viewModel = new BookmarkViewModel
            {
                Items = bookmarkedItems
            };
            return View(viewModel);
        }

        // POST: Bookmark/Add/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            // Add to bookmarks (stored in session)
            var bookmarks = GetBookmarks();
            if (!bookmarks.Contains(id))
            {
                bookmarks.Add(id);
                SaveBookmarks(bookmarks);
            }

            // Return JSON for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, bookmarkCount = bookmarks.Count });
            }

            return RedirectToAction("Index");
        }

        // POST: Bookmark/Remove/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int id)
        {
            var bookmarks = GetBookmarks();
            if (bookmarks.Contains(id))
            {
                bookmarks.Remove(id);
                SaveBookmarks(bookmarks);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, bookmarkCount = bookmarks.Count });
            }

            return RedirectToAction("Index");
        }

        // GET: Bookmark/GetCount
        [HttpGet]
        public IActionResult GetCount()
        {
            var bookmarks = GetBookmarks();
            var count = bookmarks.Count;
            return Json(new { count = count });
        }

        private List<int> GetBookmarks()
        {
            var bookmarksJson = HttpContext.Session.GetString("Bookmarks");
            if (string.IsNullOrEmpty(bookmarksJson))
            {
                return new List<int>();
            }

            return System.Text.Json.JsonSerializer.Deserialize<List<int>>(bookmarksJson) ?? new List<int>();
        }

        private void SaveBookmarks(List<int> bookmarks)
        {
            var bookmarksJson = System.Text.Json.JsonSerializer.Serialize(bookmarks);
            HttpContext.Session.SetString("Bookmarks", bookmarksJson);
        }

        private List<BookmarkItemViewModel> GetBookmarkedItems()
        {
            var bookmarkIds = GetBookmarks();
            var products = _context.Products.Where(p => bookmarkIds.Contains(p.Id)).ToList();

            var bookmarkedItems = new List<BookmarkItemViewModel>();
            foreach (var bookmarkId in bookmarkIds)
            {
                var product = products.FirstOrDefault(p => p.Id == bookmarkId);
                if (product != null)
                {
                    bookmarkedItems.Add(new BookmarkItemViewModel
                    {
                        Product = product
                    });
                }
            }

            return bookmarkedItems;
        }
    }

    public class BookmarkViewModel
    {
        public List<BookmarkItemViewModel> Items { get; set; } = new List<BookmarkItemViewModel>();
    }

    public class BookmarkItemViewModel
    {
        public Product Product { get; set; }
    }
}