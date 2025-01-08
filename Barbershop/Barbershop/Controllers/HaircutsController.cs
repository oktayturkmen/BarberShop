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
    // Sadece Admin rolüne sahip kullanıcıların erişebileceği bir denetleyici
    [Authorize(Roles = "Admin")]
    public class HaircutsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor: DbContext nesnesi ile bağlantı kurulur
        public HaircutsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Haircuts - Saç kesimlerinin listesini görüntüler
        public async Task<IActionResult> Index()
        {
            // Saç kesimlerinin listesi var mı kontrol edilir ve asenkron olarak veri çekilir
            return _context.Haircut != null ?
                View(await _context.Haircut.ToListAsync()) :
                Problem("Entity set 'ApplicationDbContext.Haircut' is null.");
        }

        // GET: Haircuts/Details/5 - Belirli bir saç kesiminin detaylarını gösterir
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Haircut == null)
            {
                // id yoksa veya Haircut tablosu boşsa hata döndürülür
                return NotFound();
            }

            // Saç kesimi, verilen id'ye göre bulunur
            var haircut = await _context.Haircut
                .FirstOrDefaultAsync(m => m.HaircutId == id);
            if (haircut == null)
            {
                // Saç kesimi bulunamazsa hata döndürülür
                return NotFound();
            }

            // Saç kesiminin detayları gösterilir
            return View(haircut);
        }

        // GET: Haircuts/Create - Yeni saç kesimi eklemek için formu gösterir
        public IActionResult Create()
        {
            return View();
        }

        // POST: Haircuts/Create - Yeni saç kesimi oluşturulması işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HaircutId,Name,Price,Duration")] Haircut haircut)
        {
            if (ModelState.IsValid)
            {
                // Model geçerli ise veritabanına eklenir
                _context.Add(haircut);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Başarılı ekleme sonrası listeye yönlendirilir
            }
            return View(haircut); // Hata durumunda tekrar form gösterilir
        }

        // GET: Haircuts/Edit/5 - Var olan saç kesimini düzenlemek için formu gösterir
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Haircut == null)
            {
                // id yoksa veya Haircut tablosu boşsa hata döndürülür
                return NotFound();
            }

            // Saç kesimi, verilen id'ye göre bulunur
            var haircut = await _context.Haircut.FindAsync(id);
            if (haircut == null)
            {
                // Saç kesimi bulunamazsa hata döndürülür
                return NotFound();
            }
            return View(haircut); // Saç kesimi düzenleme formu gösterilir
        }

        // POST: Haircuts/Edit/5 - Saç kesiminin düzenlenmesi işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HaircutId,Name,Price,Duration")] Haircut haircut)
        {
            if (id != haircut.HaircutId)
            {
                // Id uyuşmazsa hata döndürülür
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Saç kesimi güncellenir
                    _context.Update(haircut);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Eğer bir hata olursa ve saç kesimi veritabanında bulunamazsa hata döndürülür
                    if (!HaircutExists(haircut.HaircutId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Diğer hatalar için hata fırlatılır
                    }
                }
                return RedirectToAction(nameof(Index)); // Başarılı güncelleme sonrası listeye yönlendirilir
            }
            return View(haircut); // Hata durumunda tekrar düzenleme formu gösterilir
        }

        // GET: Haircuts/Delete/5 - Saç kesiminin silinmesi için onay sayfası
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Haircut == null)
            {
                // id yoksa veya Haircut tablosu boşsa hata döndürülür
                return NotFound();
            }

            // Saç kesimi, verilen id'ye göre bulunur
            var haircut = await _context.Haircut
                .FirstOrDefaultAsync(m => m.HaircutId == id);
            if (haircut == null)
            {
                // Saç kesimi bulunamazsa hata döndürülür
                return NotFound();
            }

            // Saç kesiminin silinmesi için onay sayfası gösterilir
            return View(haircut);
        }

        // POST: Haircuts/Delete/5 - Saç kesiminin silinmesi işlemi
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Haircut == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Haircut' is null.");
            }
            var haircut = await _context.Haircut.FindAsync(id);
            if (haircut != null)
            {
                // Saç kesimi veritabanından silinir
                _context.Haircut.Remove(haircut);
            }

            // Değişiklikler kaydedilir
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // Silme sonrası listeye yönlendirilir
        }

        // Saç kesiminin var olup olmadığını kontrol eden yardımcı metod
        private bool HaircutExists(int id)
        {
            return (_context.Haircut?.Any(e => e.HaircutId == id)).GetValueOrDefault();
        }
    }
}
