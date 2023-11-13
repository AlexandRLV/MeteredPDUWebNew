using Microsoft.AspNetCore.Mvc;

namespace MeteredPDUWebNew;

public class HomeController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}