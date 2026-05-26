using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RepresentWeb.Models;
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

    public IActionResult Prestige()
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


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
