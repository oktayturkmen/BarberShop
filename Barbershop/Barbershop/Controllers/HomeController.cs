using Barbershop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


namespace Barbershop.Controllers
{
    public class HomeController : Controller // HomeController, Controller sınıfından türetilir.
    {
        private readonly ILogger<HomeController> _logger; // Loglama işlemleri için kullanılan bir logger nesnesi.

        // Constructor (yapıcı metot) - Logger bağımlılığını alır ve sınıf seviyesindeki değişkene atar.
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger; // Logger bağımlılığını kullanmak üzere _logger değişkenine atar.
        }
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Kullanıcı giriş yaptıysa Dashboard view'ını döndür
                return View("Dashboard");
            }
            else
            {
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }
        // Hata sayfası (Error) aksiyonu
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Hata detaylarını ErrorViewModel'e aktarır ve Error görünümünü döndürür.
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}