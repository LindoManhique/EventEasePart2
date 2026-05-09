using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;

        public EventController(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Events.FirstOrDefaultAsync(x => x.EventID == id);
            if (item == null) return NotFound();

            return View(item);
        }

        public IActionResult Create()
        {
            return View();
        }

        // CREATE FIXED
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventItem, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View(eventItem);

            if (imageFile != null && imageFile.Length > 0)
                eventItem.ImageUrl = await _blobService.UploadFileAsync(imageFile);

            _context.Add(eventItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Events.FindAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        // EDIT FIXED
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event formModel, IFormFile? imageFile, bool deleteImage)
        {
            var item = await _context.Events.FirstOrDefaultAsync(x => x.EventID == id);
            if (item == null) return NotFound();

            if (!ModelState.IsValid)
                return View(formModel);

            item.Name = formModel.Name;
            item.Date = formModel.Date;
            item.Description = formModel.Description;

            if (deleteImage)
            {
                if (!string.IsNullOrEmpty(item.ImageUrl))
                    await _blobService.DeleteFileAsync(item.ImageUrl);

                item.ImageUrl = null;
            }
            else if (imageFile != null && imageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(item.ImageUrl))
                    await _blobService.DeleteFileAsync(item.ImageUrl);

                item.ImageUrl = await _blobService.UploadFileAsync(imageFile);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE PROTECTED
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(x => x.EventID == id);

            if (item == null) return NotFound();

            if (item.Bookings != null && item.Bookings.Any())
            {
                TempData["Error"] = "Cannot delete event with active bookings.";
                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(x => x.EventID == id);

            if (item == null) return NotFound();

            if (item.Bookings != null && item.Bookings.Any())
            {
                TempData["Error"] = "Cannot delete event with active bookings.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(item.ImageUrl))
                await _blobService.DeleteFileAsync(item.ImageUrl);

            _context.Events.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}