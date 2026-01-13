using StudentManagement.Core.Enums;
using StudentManagement.Core.Enums;

namespace StudentManagement.Core.DTOs
{
    public class CreateClassDto
    {
        public string ClassName { get; set; } // SE1801

        public int CourseId { get; set; }
        public int SemesterId { get; set; }

        public string Room { get; set; }

        // Legacy – có thể giữ để hiển thị, nhưng không dùng để check trùng lịch
        public string Schedule { get; set; } // Mon-Wed-Fri

        public int MaxCapacity { get; set; }

        // MỚI: required cho FPTU-style
        public DayOfWeekPair DayOfWeekPair { get; set; }
        public TimeSlot TimeSlot { get; set; }
    }
}
