using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Barbershop.Controllers
{
    [Route("api/[controller]")] // API'nin route'u "/api/userapi" olarak tanımlanır
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager; // Kullanıcı yönetimi için bağımlılık

        public UserApiController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager; // Bağımlılığı sınıfa atar
        }

        // Kullanıcıların listesini döndüren endpoint
        [HttpGet]
        public IActionResult GetUsers()
        {
            // Tüm kullanıcıları listele ve yalnızca gerekli bilgileri seç
            var users = _userManager.Users.Select(u => new
            {
                u.Id,         // Kullanıcı ID'si
                u.UserName,   // Kullanıcı adı
                u.Email       // E-posta adresi
            }).ToList();

            return Ok(users); // Kullanıcıları JSON formatında döndür
        }

        // Belirli bir kullanıcıyı silen endpoint
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            // Verilen ID'ye sahip kullanıcıyı bul
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" }); // Kullanıcı bulunamazsa hata döndür
            }

            // Kullanıcıyı sil
            await _userManager.DeleteAsync(user);
            return Ok(new { Message = "User deleted successfully" }); // Başarı mesajı döndür
        }
    }
}
