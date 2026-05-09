using EventEase.Data;
using EventEase.Models;
using Microsoft.EntityFrameworkCore;
using EventEase.Services; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔥 ADD BLOB SERVICE (IMPORTANT FOR AZURE STORAGE)
builder.Services.AddSingleton<BlobService>();

var app = builder.Build();


// Only run DB creation + seeding locally (Development)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        if (!context.Venues.Any())
        {
            var venues = new Venue[]
            {
                new Venue { Location = "Cape Town Convention Center", Capacity = 500, ImageUrl = "venue1.jpg" },
                new Venue { Location = "Johannesburg Expo Hall", Capacity = 300, ImageUrl = "venue2.jpg" }
            };
            context.Venues.AddRange(venues);
            context.SaveChanges();
        }

        if (!context.Events.Any())
        {
            var events = new Event[]
            {
                new Event { Name = "Tech Conference", Date = DateTime.Now.AddDays(30), Description = "Annual tech conference" },
                new Event { Name = "Music Festival", Date = DateTime.Now.AddDays(60), Description = "Live music festival" }
            };
            context.Events.AddRange(events);
            context.SaveChanges();
        }

        if (!context.Bookings.Any())
        {
            var firstVenue = context.Venues.First();
            var firstEvent = context.Events.First();

            var bookings = new Booking[]
            {
                new Booking{ VenueID = firstVenue.VenueID, EventID = firstEvent.EventID, BookingDate = DateTime.Now }
            };
            context.Bookings.AddRange(bookings);
            context.SaveChanges();
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();