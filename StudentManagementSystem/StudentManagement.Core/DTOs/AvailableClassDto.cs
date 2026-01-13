using StudentManagement.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Core.DTOs
{
    public class AvailableClassDto
    {
        public int ClassId { get; set; }
        public string ClassCode { get; set; }
        public string ClassName { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Credits { get; set; }

        public string Room { get; set; }
        public string Schedule { get; set; }
        public DayOfWeekPair DayOfWeekPair { get; set; }
        public TimeSlot TimeSlot { get; set; }

        public int CurrentEnrollment { get; set; }
        public int MaxCapacity { get; set; }

        // trạng thái cho UI
        public bool CanRegister { get; set; }
        public string StatusText { get; set; }   // "Đủ chỗ", "Hết chỗ", "Không đủ điều kiện"
    }
}
