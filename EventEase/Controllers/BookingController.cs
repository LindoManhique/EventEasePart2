using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🟩 INDEX
        public async Task<IActionResult> Index()
        {
            var bookings = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue);

            return View(await bookings.ToListAsync());
        }

        // 🟩 DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // 🟩 CREATE (GET)
        public IActionResult Create()
        {
            ViewData["EventID"] = new SelectList(_context.Events, "EventID", "Name");
            ViewData["VenueID"] = new SelectList(_context.Venues, "VenueID", "Location");
            return View();
        }

        // 🟨 CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            bool exists = _context.Bookings.Any(b =>
                b.VenueID == booking.VenueID &&
                b.BookingDate.Date == booking.BookingDate.Date);

            if (exists)
            {
                ModelState.AddModelError("", "This venue is already booked on this date.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["EventID"] = new SelectList(_context.Events, "EventID", "Name");
                ViewData["VenueID"] = new SelectList(_context.Venues, "VenueID", "Location");
                return View(booking);
            }

            _context.Add(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 🟩 EDIT (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            ViewData["EventID"] = new SelectList(_context.Events, "EventID", "Name", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venues, "VenueID", "Location", booking.VenueID);

            return View(booking);
        }

        // 🟨 EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingID) return NotFound();

            bool exists = _context.Bookings.Any(b =>
                b.VenueID == booking.VenueID &&
                b.BookingDate.Date == booking.BookingDate.Date &&
                b.BookingID != booking.BookingID);

            if (exists)
            {
                ModelState.AddModelError("", "This venue is already booked on this date.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["EventID"] = new SelectList(_context.Events, "EventID", "Name");
                ViewData["VenueID"] = new SelectList(_context.Venues, "VenueID", "Location");
                return View(booking);
            }

            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 🟥 DELETE (GET) — FIXED
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // 🟥 DELETE (POST) — FIXED (THIS WAS YOUR ISSUE)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null)
            {
                return NotFound();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 🟦 BOOKING REPORT
        public async Task<IActionResult> BookingReport(string? search)
        {
            var query = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    b.Event.Name.Contains(search) ||
                    b.BookingID.ToString().Contains(search));
            }

            var data = await query
                .Select(b => new BookingReportViewModel
                {
                    BookingID = b.BookingID,
                    EventName = b.Event != null ? b.Event.Name : "N/A",
                    VenueName = b.Venue != null ? b.Venue.Location : "N/A",
                    BookingDate = b.BookingDate
                })
                .ToListAsync();

            return View(data);
        }
    }
}