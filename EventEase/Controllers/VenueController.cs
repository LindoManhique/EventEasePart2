using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Controllers
{
    public class VenueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;

        public VenueController(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Venues.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Venues.FirstOrDefaultAsync(x => x.VenueID == id);
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
        public async Task<IActionResult> Create(Venue venue, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View(venue);

            if (imageFile != null && imageFile.Length > 0)
                venue.ImageUrl = await _blobService.UploadFileAsync(imageFile);

            _context.Add(venue);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Venues.FindAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        // EDIT FIXED (IMPORTANT PART)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue formModel, IFormFile? imageFile, bool deleteImage)
        {
            var item = await _context.Venues.FirstOrDefaultAsync(x => x.VenueID == id);
            if (item == null) return NotFound();

            if (!ModelState.IsValid)
                return View(formModel);

            item.Location = formModel.Location;
            item.Capacity = formModel.Capacity;

            // DELETE IMAGE
            if (deleteImage)
            {
                if (!string.IsNullOrEmpty(item.ImageUrl))
                    await _blobService.DeleteFileAsync(item.ImageUrl);

                item.ImageUrl = null;
            }
            // UPLOAD IMAGE
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

            var item = await _context.Venues
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(x => x.VenueID == id);

            if (item == null) return NotFound();

            if (item.Bookings != null && item.Bookings.Any())
            {
                TempData["Error"] = "Cannot delete venue with active bookings.";
                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Venues
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(x => x.VenueID == id);

            if (item == null) return NotFound();

            if (item.Bookings != null && item.Bookings.Any())
            {
                TempData["Error"] = "Cannot delete venue with active bookings.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(item.ImageUrl))
                await _blobService.DeleteFileAsync(item.ImageUrl);

            _context.Venues.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}