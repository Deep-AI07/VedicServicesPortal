using System;
using System.Collections.Generic;
using VedicServicesPortal.Models;

namespace VedicServicesPortal.ViewModels
{
    public class BookingViewModel
    {
        // Selected Service
        public Service Service { get; set; }

        // Available Pandits
        public List<Pandit> Pandits { get; set; }

        // Available Time Slots
        public List<TimeSlot> TimeSlots { get; set; }

        // Selected Service ID
        public int ServiceId { get; set; }

        // Selected Pandit ID
        public int PanditId { get; set; }

        // Selected Slot ID
        public int SlotId { get; set; }

        // Customer Address
        public string Address { get; set; }

        // Booking Date (User selected date for ceremony)
        public DateTime BookingDate { get; set; } = DateTime.Today.AddDays(1);

        // Auspicious Time Slot selected by user
        public string StartTime { get; set; }

        public string EndTime { get; set; }

        // Payment Method Details (UPI, Card, Cash on Completion)
        public string PaymentMethod { get; set; } = "UPI";

        public string UpiId { get; set; }

        public string CardHolderName { get; set; }

        public string CardNumber { get; set; }

        public string CardExpiry { get; set; }

        public string CardCvv { get; set; }

        public string TransactionId { get; set; }

        public bool IsPaymentCompleted { get; set; }
    }
}
