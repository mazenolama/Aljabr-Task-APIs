using System;
using SlotManagement.Enums;

namespace SlotManagement.Models
{
    public class Slot
    {
        public int SlotId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; } = true;
        public SlotStatus Status { get; set; } = SlotStatus.Available;
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;
        public bool Deleted { get; set; } = false;
    }
}
