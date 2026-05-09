using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Booking
    {
        public int BookingID { get; set; }

        [Required]
        public int EventID { get; set; }

        public Event? Event { get; set; }

        [Required]
        public int VenueID { get; set; }

        public Venue? Venue { get; set; }

        [Required(ErrorMessage = "Booking date is required")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        //  BUSINESS RULE VALIDATION (prevents past bookings)
        public bool IsValidDate()
        {
            return BookingDate.Date >= DateTime.Today;
        }
    }
}