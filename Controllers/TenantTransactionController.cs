using Microsoft.AspNetCore.Mvc;

namespace Invi.Controllers
{
    public class TenantTransactionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
