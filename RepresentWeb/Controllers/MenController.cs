using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RepresentWeb.Models;

namespace RepresentWeb.Controllers;

public class MenController : Controller {

private readonly ILogger<MenController> _logger;

    public MenController(ILogger<MenController> logger)
    {
        _logger = logger;
    }

    [HttpGet("Men")]
     public IActionResult Men()
    {
        return View();
    }

}