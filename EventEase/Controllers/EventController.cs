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

        // 
        public async Task<IActionResult> Index(int? eventTypeId, DateTime? fromDate, DateTime? toDate)
        {
            var eventsQuery = _context.Events
                .Include(e => e.EventType)
                .AsQueryable();

            // 
            if (eventTypeId.HasValue && eventTypeId > 0)
            {
                eventsQuery = eventsQuery.Where(e => e.EventTypeID == eventTypeId);
            }

            // 
            if (fromDate.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.Date <= toDate.Value);
            }

            ViewBag.EventTypes = await _context.EventTypes.ToListAsync();
            ViewBag.SelectedEventType = eventTypeId;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(await eventsQuery.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(x => x.EventID == id);

            if (item == null) return NotFound();

            return View(item);
        }

        public IActionResult Create()
        {
            ViewBag.EventTypes = _context.EventTypes.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventItem, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EventTypes = _context.EventTypes.ToList();
                return View(eventItem);
            }

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

            ViewBag.EventTypes = _context.EventTypes.ToList();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event formModel, IFormFile? imageFile, bool deleteImage)
        {
            var item = await _context.Events.FirstOrDefaultAsync(x => x.EventID == id);
            if (item == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.EventTypes = _context.EventTypes.ToList();
                return View(formModel);
            }

            item.Name = formModel.Name;
            item.Date = formModel.Date;
            item.Description = formModel.Description;
            item.EventTypeID = formModel.EventTypeID;

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