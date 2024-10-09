using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Quizer.Models;
using Quizer.Models.User;
using System.Diagnostics;

namespace Quizer.Controllers
{
    public class HomeController(
        ILogger<HomeController> logger, UserManager<ApplicationUser> userManager
        ) : Controller
    {

        public IActionResult Index()
        {
            if (User.Identity == null)
            {
                return Unauthorized();
            }

            if (User.Identity.IsAuthenticated && userManager.GetUserAsync(User) != null)
            {
                return RedirectToAction("Index", "Quiz");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
