using VedicServicesPortal.Models;

namespace VedicServicesPortal.ViewModels
{
    public class BookingConfirmationViewModel
    {
        public Booking Booking { get; set; }

        public Service Service { get; set; }

        public Pandit Pandit { get; set; }

        public TimeSlot Slot { get; set; }
    }
}
