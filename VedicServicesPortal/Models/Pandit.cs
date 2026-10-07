using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VedicServicesPortal.Models
{
    public class Pandit
    {
        [Key]
        public int PanditId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Mobile { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public int Experience { get; set; }

        public string Specialization { get; set; }

        public bool IsAvailable { get; set; }

        public ICollection<TimeSlot> TimeSlots { get; set; }

        public ICollection<Booking> Bookings { get; set; }
    }
}
