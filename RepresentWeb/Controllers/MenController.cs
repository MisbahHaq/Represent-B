using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using representweb.Data;
using RepresentWeb.Models;

namespace RepresentWeb.Controllers;

public class MenController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MenController> _logger;

    public MenController(ApplicationDbContext context, ILogger<MenController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("Men")]
    public async Task<IActionResult> Men()
    {
        var products = await _context.Products.ToListAsync();

        return View(products);
    }
}