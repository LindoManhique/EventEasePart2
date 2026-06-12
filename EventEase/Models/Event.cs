using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        public int EventID { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        [StringLength(200, ErrorMessage = "Event name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int EventTypeID { get; set; } = 1;

        public EventType? EventType { get; set; }

        public string? ImageUrl { get; set; }

        public List<Booking> Bookings { get; set; } = new();
    }
}