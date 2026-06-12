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

        // PART 3 REQUIRED TABLE
        public DbSet<EventType> EventTypes { get; set; }

        // REPORT VIEW MODEL (KEYLESS)
        public DbSet<BookingReportViewModel> BookingReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BookingReportViewModel>()
                .HasNoKey();

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.EventType)
                .WithMany(t => t.Events)
                .HasForeignKey(e => e.EventTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventType>()
                .HasData(
                    new EventType { EventTypeID = 1, Name = "Conference" },
                    new EventType { EventTypeID = 2, Name = "Wedding" },
                    new EventType { EventTypeID = 3, Name = "Concert" },
                    new EventType { EventTypeID = 4, Name = "Seminar" },
                    new EventType { EventTypeID = 5, Name = "Expo" }
                );
        }
    }
}