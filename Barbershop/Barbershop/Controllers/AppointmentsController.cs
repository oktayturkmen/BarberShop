using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Barbershop.Data;
using Barbershop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Barbershop.Controllers
{
    [Authorize] // Bu kontrolcüye erişim yetkilendirme gerektirir.
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Giriş yapan kullanıcının randevularını listeleyen metod
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // Şu anki giriş yapan kullanıcıyı al
            var userId = user.Id; // Kullanıcının ID'sini al
            var applicationDbContext = _context.Appointments
                .Include(a => a.Barber) // Berberi dahil et
                .Include(a => a.Haircut) // Saç kesimini dahil et
                .Where(a => a.CustomerId == userId); // Sadece giriş yapan kullanıcının randevularını filtrele
            return View(await applicationDbContext.ToListAsync());
        }

        // Tüm kullanıcıların randevularını listeleyen metod
        public async Task<IActionResult> ListAll()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Barber) // Berberi dahil et
                .Include(a => a.Haircut) // Saç kesimini dahil et
                .Include(a => a.User) // Kullanıcıyı dahil et
                .ToListAsync();

            return View(appointments);
        }

        // Randevuyu onaylama işlemi
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id); // Randevuyu ID'ye göre bul
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Confirmed; // Durumu "onaylandı" olarak ayarla
                await _context.SaveChangesAsync(); // Veritabanını güncelle
            }
            return RedirectToAction(nameof(ListAll)); // Listeleme sayfasına yönlendir
        }

        // Randevuyu reddetme işlemi
        public async Task<IActionResult> RejectAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id); // Randevuyu ID'ye göre bul
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Rejected; // Durumu "reddedildi" olarak ayarla
                await _context.SaveChangesAsync(); // Veritabanını güncelle
            }
            return RedirectToAction(nameof(ListAll)); // Listeleme sayfasına yönlendir
        }

        // Randevuyu iptal etme işlemi
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id); // Randevuyu ID'ye göre bul
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Rejected; // Durumu "reddedildi" olarak ayarla
                await _context.SaveChangesAsync(); // Veritabanını güncelle
            }
            return RedirectToAction(nameof(ListAll)); // Listeleme sayfasına yönlendir
        }

        // Belirli bir randevunun detaylarını görüntüleme
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound(); // Randevu bulunamazsa 404 döndür
            }

            var appointment = await _context.Appointments
                .Include(a => a.Barber) // Berberi dahil et
                .Include(a => a.Haircut) // Saç kesimini dahil et
                .FirstOrDefaultAsync(m => m.AppointmentId == id); // ID'ye göre randevuyu bul
            if (appointment == null)
            {
                return NotFound(); // Randevu bulunamazsa 404 döndür
            }

            return View(appointment); // Randevuyu görüntüle
        }

        // Yeni bir randevu oluşturma sayfasını görüntüleme
        public IActionResult Create()
        {
            ViewData["BarberId"] = new SelectList(_context.Barbers, "BarberId", "BarberName"); // Berber listesini doldur
            ViewData["HaircutId"] = new SelectList(_context.Haircut, "HaircutId", "Name"); // Saç kesim listesini doldur
            ViewBag.BookedTimes = new List<DateTime>(); // Randevu zamanlarını boş bir listeyle başlat
            return View();
        }

        // Yeni bir randevu oluşturma işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentId,AppointmentDateTime,BarberId,HaircutId,CustomerId")] Appointment appointment)
        {
            var user = await _userManager.GetUserAsync(User); // Giriş yapan kullanıcıyı al
            appointment.CustomerId = user.Id; // Randevuyu bu kullanıcıya bağla

            if (ModelState.IsValid) // Form verisi doğruysa
            {
                _context.Add(appointment); // Randevuyu veritabanına ekle
                await _context.SaveChangesAsync(); // Veritabanını güncelle
                return RedirectToAction(nameof(Index)); // Kullanıcının randevularını listele
            }
            ViewData["BarberId"] = new SelectList(_context.Barbers, "BarberId", "BarberId", appointment.BarberId); // Formu yeniden doldur
            ViewData["HaircutId"] = new SelectList(_context.Haircut, "HaircutId", "HaircutId", appointment.HaircutId);
            return View(appointment);
        }

        // Randevuyu düzenleme sayfasını görüntüleme
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound(); // Randevu bulunamazsa 404 döndür
            }

            var appointment = await _context.Appointments.FindAsync(id); // Randevuyu ID'ye göre bul
            if (appointment == null)
            {
                return NotFound(); // Randevu bulunamazsa 404 döndür
            }
            ViewData["BarberId"] = new SelectList(_context.Barbers, "BarberId", "BarberName", appointment.BarberId); // Formu doldur
            ViewData["HaircutId"] = new SelectList(_context.Haircut, "HaircutId", "Name", appointment.HaircutId);
            return View(appointment);
        }

        // Randevuyu düzenleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AppointmentId,AppointmentDateTime,BarberId,HaircutId,CustomerId")] Appointment appointment)
        {
            if (id != appointment.AppointmentId)
            {
                return NotFound(); // ID uyuşmazsa 404 döndür
            }

            if (ModelState.IsValid) // Form verisi doğruysa
            {
                try
                {
                    _context.Update(appointment); // Randevuyu güncelle
                    await _context.SaveChangesAsync(); // Veritabanını güncelle
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.AppointmentId))
                    {
                        return NotFound(); // Randevu bulunamazsa 404 döndür
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index)); // Kullanıcının randevularını listele
            }
            ViewData["BarberId"] = new SelectList(_context.Barbers, "BarberId", "BarberId", appointment.BarberId); // Formu yeniden doldur
            ViewData["HaircutId"] = new SelectList(_context.Haircut, "HaircutId", "HaircutId", appointment.HaircutId);
            return View(appointment);
        }

        // Randevuyu silme sayfasını görüntüleme
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound(); // Randevu bulunamazsa 404 döndür
            }

            var appointment = await _context.Appointments
                .Include(a => a.Barber) // Berberi dahil et
                .Include(a => a.Haircut) // Saç kesimini dahil et
                .FirstOrDefaultAsync(m => m.AppointmentId == id); // ID'ye göre randevuyu bul
            if (appointment == null)
            {
                return NotFound(); // Randevu bulunamazsa 404 döndür
            }

            return View(appointment); // Silme sayfasını görüntüle
        }

        // Randevuyu silme işlemi
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Appointments == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointments'  is null."); // Veritabanı boşsa hata döndür
            }
            var appointment = await _context.Appointments.FindAsync(id); // Randevuyu ID'ye göre bul
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment); // Randevuyu sil
            }

            await _context.SaveChangesAsync(); // Veritabanını güncelle
            return RedirectToAction(nameof(Index)); // Kullanıcının randevularını listele
        }

        // Berberin belirli bir tarihteki uygun zamanlarını getiren metod
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimes(DateTime date, int barberId)
        {
            var today = DateTime.Today;
            var now = DateTime.Now;
            // Eğer seçilen tarih bugünün tarihi ise başlangıç saatini mevcut saate ayarla
            var startTime = date.Date == today ? new DateTime(today.Year, today.Month, today.Day, now.Hour, now.Minute, 0).AddMinutes(40 - (now.Minute % 40)) : date.Date.AddHours(9);
            var endTime = date.Date.AddHours(17); // Çalışma bitiş saatini belirle (17:00)

            // Seçilen tarihte berberin dolu olduğu saatleri al
            var bookedTimes = await _context.Appointments
                .Where(a => a.BarberId == barberId && a.AppointmentDateTime.Date == date.Date)
                .Select(a => a.AppointmentDateTime)
                .ToListAsync();

            // O gün için olası tüm zaman aralıklarını oluştur
            var timeSlots = Enumerable.Range(0, (int)(endTime - startTime).TotalMinutes / 40)
                .Select(i => startTime.AddMinutes(i * 40))
                .ToList();

            // Dolu olmayan zaman aralıklarını seç
            var availableTimes = timeSlots
                .Where(t => !bookedTimes.Contains(t))
                .Select(t => new { value = t.ToString("o"), text = t.ToString("HH:mm") })
                .ToList();

            return Json(availableTimes); // Uygun zaman aralıklarını JSON formatında döndür
        }

        // Belirli bir randevunun var olup olmadığını kontrol eden metod
        private bool AppointmentExists(int id)
        {
            return (_context.Appointments?.Any(e => e.AppointmentId == id)).GetValueOrDefault();
        }
    }
}
