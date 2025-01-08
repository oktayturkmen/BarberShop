using Microsoft.AspNetCore.Mvc;

namespace Barbershop.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Kullanıcı listesi için Index görünümünü yükle
        }
    }
}
