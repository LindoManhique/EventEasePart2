using System;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Models
{
    [Keyless]
    public class BookingReportViewModel
    {
        public int BookingID { get; set; }
        public string? EventName { get; set; }
        public string? VenueName { get; set; }
        public DateTime BookingDate { get; set; }
    }
}