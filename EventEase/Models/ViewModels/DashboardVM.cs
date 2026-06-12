using System.Collections.Generic;

namespace EventEase.Models.ViewModels
{
    public class DashboardVM
    {
        //  summary stats
        public int TotalEventsAll { get; set; }
        public int TotalVenues { get; set; }
        public int AvailableVenues { get; set; }
        public int TotalBookings { get; set; }

        //  chart/table data
        public List<EventTypeStatItem> EventTypeStats { get; set; }

        public List<VenueUtilisationVM> VenueUtilisation { get; set; }
    }

     
    public class EventTypeStatItem
    {
        public string EventTypeName { get; set; }
        public int TotalEvents { get; set; }
    }
}