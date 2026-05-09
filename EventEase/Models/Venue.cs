using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        public int VenueID { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(150, ErrorMessage = "Location cannot exceed 150 characters")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 100000, ErrorMessage = "Capacity must be greater than 0")]
        public int Capacity { get; set; }

        // Azure Blob Storage Image
        public string? ImageUrl { get; set; }

        // FIX: avoid null reference crashes
        public List<Booking> Bookings { get; set; } = new();
    }
}