using System;
using System.ComponentModel.DataAnnotations;

namespace VedicServicesPortal.Models
{
    public class TimeSlot
    {
        [Key]
        public int SlotId { get; set; }

        [Required]
        public int PanditId { get; set; }

        public Pandit Pandit { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public string StartTime { get; set; }

        [Required]
        public string EndTime { get; set; }

        public bool IsBooked { get; set; }
    }
}
