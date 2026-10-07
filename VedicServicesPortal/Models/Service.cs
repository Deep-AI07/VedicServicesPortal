using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VedicServicesPortal.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        public string ServiceName { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(1, 100000)]
        public decimal Price { get; set; }

        public int Duration { get; set; }

        public bool IsActive { get; set; }

        public ICollection<Booking> Bookings { get; set; }
    }
}
