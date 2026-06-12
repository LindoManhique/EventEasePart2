using Microsoft.AspNetCore.Mvc;
using EventEase.Data;
using EventEase.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = new DashboardVM
            {
                // OVERALL STATS
                TotalEventsAll = await _context.Events.CountAsync(),
                TotalVenues = await _context.Venues.CountAsync(),
                AvailableVenues = await _context.Venues.CountAsync(v => v.IsAvailable),
                TotalBookings = await _context.Bookings.CountAsync()
            };

            // EVENTS PER TYPE
            dashboard.EventTypeStats = await _context.Events
                .Include(e => e.EventType)
                .GroupBy(e => e.EventType.Name)
                .Select(g => new EventTypeStatItem
                {
                    EventTypeName = g.Key,
                    TotalEvents = g.Count()
                })
                .ToListAsync();

            // VENUE UTILISATION (NEW ADDITION)
            var totalBookings = await _context.Bookings.CountAsync();

            dashboard.VenueUtilisation = await _context.Venues
                .Include(v => v.Bookings)
                .Select(v => new VenueUtilisationVM
                {
                    VenueName = v.Location,
                    BookingCount = v.Bookings.Count,
                    UtilisationPercent = totalBookings == 0
                        ? 0
                        : (double)v.Bookings.Count / totalBookings * 100
                })
                .ToListAsync();

            return View(dashboard);
        }
    }
}