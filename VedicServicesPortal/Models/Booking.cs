using System;
using System.ComponentModel.DataAnnotations;

namespace VedicServicesPortal.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        // User
        public int UserId { get; set; }
        public User User { get; set; }

        // Pandit
        public int PanditId { get; set; }
        public Pandit Pandit { get; set; }

        // Service
        public int ServiceId { get; set; }
        public Service Service { get; set; }

        // Time Slot
        public int SlotId { get; set; }
        public TimeSlot TimeSlot { get; set; }

        // Booking information
        public DateTime BookingDate { get; set; }

        [Required]
        public string Address { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; }

        public string PaymentMethod { get; set; } = "COD";

        public DateTime CreatedAt { get; set; }
    }
}
