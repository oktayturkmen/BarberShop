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

namespace Barbershop.Controllers
{
    // Sadece Admin rolüne sahip kullanıcılar erişebilir
    [Authorize(Roles = "Admin")]
    public class BarbersController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Controller'ın constructor'ı, veritabanı bağlamını (DbContext) alır.
        public BarbersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Barbers (Kuaförlerin listelendiği sayfa)
        public async Task<IActionResult> Index()
        {
            // Veritabanındaki kuaförleri alfabetik olarak sıralar ve liste olarak alır.
            var orderedBarbers = await _context.Barbers
                                             .OrderBy(b => b.BarberName)
                                             .ToListAsync();
            return View(orderedBarbers); // Listeyi görünümde (view) gösterir.
        }

        // GET: Barbers/Details/5 (Belirli bir kuaförün detaylarının gösterildiği sayfa)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Barbers == null)
            {
                return NotFound(); // ID parametresi eksik veya kuaförler veritabanı boşsa hata döner.
            }

            // Veritabanından belirli bir kuaförü alır.
            var barber = await _context.Barbers
                .FirstOrDefaultAsync(m => m.BarberId == id);
            if (barber == null)
            {
                return NotFound(); // Kuaför bulunamazsa hata döner.
            }

            return View(barber); // Kuaförün detaylarını gösterir.
        }

        // GET: Barbers/Create (Yeni kuaför eklemek için form gösterir)
        public IActionResult Create()
        {
            return View(); // Yeni kuaför eklemek için boş bir form gösterir.
        }

        // POST: Barbers/Create (Yeni kuaför ekleme işlemi)
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF saldırılarına karşı korunma
        public async Task<IActionResult> Create([Bind("BarberId,BarberName,Specialization")] Barber barber)
        {
            if (ModelState.IsValid)
            {
                // Geçerli model ise kuaförü ekler ve veritabanına kaydeder.
                _context.Add(barber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Başarıyla eklenince listeye geri döner.
            }
            else
            {
                // Model geçerli değilse, hataları konsola yazdırır.
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                Console.WriteLine(errors);
            }
            return View(barber); // Hatalı modelle formu tekrar gösterir.
        }

        // GET: Barbers/Edit/5 (Bir kuaförün bilgilerini düzenlemek için formu gösterir)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Barbers == null)
            {
                return NotFound(); // ID parametresi eksik veya kuaförler veritabanı boşsa hata döner.
            }

            // Veritabanından belirli bir kuaförü alır.
            var barber = await _context.Barbers.FindAsync(id);
            if (barber == null)
            {
                return NotFound(); // Kuaför bulunamazsa hata döner.
            }
            return View(barber); // Kuaför bilgileriyle formu gösterir.
        }

        // POST: Barbers/Edit/5 (Bir kuaförün bilgilerini günceller)
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF saldırılarına karşı korunma
        public async Task<IActionResult> Edit(int id, [Bind("BarberId,BarberName,Specialization")] Barber barber)
        {
            if (id != barber.BarberId)
            {
                return NotFound(); // ID eşleşmezse hata döner.
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Veritabanındaki kuaförü günceller.
                    _context.Update(barber);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Eşzamanlılık hatası oluşursa, kuaförün var olup olmadığını kontrol eder.
                    if (!BarberExists(barber.BarberId))
                    {
                        return NotFound(); // Kuaför bulunamazsa hata döner.
                    }
                    else
                    {
                        throw; // Diğer hataları tekrar fırlatır.
                    }
                }
                return RedirectToAction(nameof(Index)); // Başarıyla güncellenince listeye geri döner.
            }
            return View(barber); // Hatalı modelle formu tekrar gösterir.
        }

        // GET: Barbers/Delete/5 (Bir kuaförü silmek için onay formu gösterir)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Barbers == null)
            {
                return NotFound(); // ID parametresi eksik veya kuaförler veritabanı boşsa hata döner.
            }

            // Veritabanından belirli bir kuaförü alır.
            var barber = await _context.Barbers
                .FirstOrDefaultAsync(m => m.BarberId == id);
            if (barber == null)
            {
                return NotFound(); // Kuaför bulunamazsa hata döner.
            }

            return View(barber); // Silme onayı için kuaför bilgilerini gösterir.
        }

        // POST: Barbers/Delete/5 (Kuaförü siler)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] // CSRF saldırılarına karşı korunma
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Barbers == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Barbers' is null.");
            }
            // Veritabanından kuaförü bulur ve siler.
            var barber = await _context.Barbers.FindAsync(id);
            if (barber != null)
            {
                _context.Barbers.Remove(barber);
            }

            await _context.SaveChangesAsync(); // Değişiklikleri kaydeder.
            return RedirectToAction(nameof(Index)); // Silme işleminden sonra listeye geri döner.
        }

        // Kuaförün veritabanında var olup olmadığını kontrol eder.
        private bool BarberExists(int id)
        {
            return (_context.Barbers?.Any(e => e.BarberId == id)).GetValueOrDefault();
        }
    }
}
