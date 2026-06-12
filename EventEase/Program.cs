using EventEase.Data;
using EventEase.Models;
using Microsoft.EntityFrameworkCore;
using EventEase.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Blob service
builder.Services.AddSingleton<BlobService>();

var app = builder.Build();

// ==========================
// DATABASE SEEDING (FIXED ORDER)
// ==========================
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Database.EnsureCreated();

        // 1. EVENT TYPES (MUST COME FIRST)
        if (!context.EventTypes.Any())
        {
            var types = new EventType[]
            {
                new EventType { Name = "Conference" },
                new EventType { Name = "Wedding" },
                new EventType { Name = "Concert" },
                new EventType { Name = "Seminar" },
                new EventType { Name = "Expo" }
            };

            context.EventTypes.AddRange(types);
            context.SaveChanges();
        }

        // 2. VENUES
        if (!context.Venues.Any())
        {
            var venues = new Venue[]
            {
                new Venue { Location = "Cape Town Convention Center", Capacity = 500, ImageUrl = "venue1.jpg", IsAvailable = true },
                new Venue { Location = "Johannesburg Expo Hall", Capacity = 300, ImageUrl = "venue2.jpg", IsAvailable = true }
            };

            context.Venues.AddRange(venues);
            context.SaveChanges();
        }

        // 3. EVENTS (NOW SAFE because EventTypes exist)
        if (!context.Events.Any())
        {
            var conferenceType = context.EventTypes.First();

            var events = new Event[]
            {
                new Event
                {
                    Name = "Tech Conference",
                    Date = DateTime.Now.AddDays(30),
                    Description = "Annual tech conference",
                    EventTypeID = conferenceType.EventTypeID
                },
                new Event
                {
                    Name = "Music Festival",
                    Date = DateTime.Now.AddDays(60),
                    Description = "Live music festival",
                    EventTypeID = conferenceType.EventTypeID
                }
            };

            context.Events.AddRange(events);
            context.SaveChanges();
        }

        // 4. BOOKINGS
        if (!context.Bookings.Any())
        {
            var firstVenue = context.Venues.First();
            var firstEvent = context.Events.First();

            var bookings = new Booking[]
            {
                new Booking
                {
                    VenueID = firstVenue.VenueID,
                    EventID = firstEvent.EventID,
                    BookingDate = DateTime.Now
                }
            };

            context.Bookings.AddRange(bookings);
            context.SaveChanges();
        }
    }
}

// ==========================
// HTTP PIPELINE
// ==========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Default route = Dashboard (Home)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();