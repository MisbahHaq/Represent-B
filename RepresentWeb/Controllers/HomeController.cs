using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RepresentWeb.Models;
using representweb.Models;
using representweb.Data;

namespace RepresentWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var bestSellers = _context.Products
            .OrderByDescending(p => p.Id)
            .Take(8)
            .ToList();
        ViewBag.BestSellers = bestSellers;
        return View();
    }

    public IActionResult Retail()
    {
        return View();
    }

    public IActionResult Vault()
    {
        return View();
    }

    public IActionResult Men()
    {
        var products = _context.Products.Where(p => p.Gender == "Men").ToList();
        return View(products);
    }

public IActionResult Women()
        {
            var products = _context.Products.Where(p => p.Gender == "Women").ToList();
            return View(products);
        }

        public IActionResult Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return View(new List<Product>());
            }

            var products = _context.Products
                .Where(p => p.Name.Contains(q) || 
                           (p.Description != null && p.Description.Contains(q)) ||
                           (p.Tags != null && p.Tags.Contains(q)))
                .ToList();

            ViewData["SearchQuery"] = q;
            return View(products);
        }

        [HttpGet]
        public JsonResult GetSuggestions(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<object>());
            }

            var suggestions = _context.Products
                .Where(p => p.Name.Contains(q) || 
                           (p.Description != null && p.Description.Contains(q)) ||
                           (p.Tags != null && p.Tags.Contains(q)))
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    image = p.ImageUrl
                })
                .Take(6)
                .ToList();

            return Json(suggestions);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
