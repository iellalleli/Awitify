using Microsoft.AspNetCore.Mvc;

public class LandingPageController : Controller
{
    public IActionResult Home()
    {
        return View();
    }
}
