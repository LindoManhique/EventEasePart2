using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class EventType
    {
        public int EventTypeID { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public List<Event> Events { get; set; } = new();
    }
}