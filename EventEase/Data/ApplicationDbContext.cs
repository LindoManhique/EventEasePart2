using Microsoft.EntityFrameworkCore;
using EventEase.Models;

namespace EventEase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        // REPORT VIEW MODEL (KEYLESS)
        public DbSet<BookingReportViewModel> BookingReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------------
            // REPORT VIEW MODEL (NO KEY)
            // -------------------------------
            modelBuilder.Entity<BookingReportViewModel>()
                .HasNoKey();

            // -------------------------------
            // RELATIONSHIP: Venue → Bookings
            // -------------------------------
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueID)
                .OnDelete(DeleteBehavior.Restrict); // ❗ prevents deleting venue with bookings

            // -------------------------------
            // RELATIONSHIP: Event → Bookings
            // -------------------------------
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventID)
                .OnDelete(DeleteBehavior.Restrict); // ❗ prevents deleting event with bookings
        }
    }
}