using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Core.Entities
{
    public class Course
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } // Lập trình C#
        public string CourseCode { get; set; } // PRN211
        public int Credits { get; set; } // Số tín chỉ: 3

        // Tự tham chiếu để làm điều kiện tiên quyết (Prerequisite)
        public int? PrerequisiteCourseId { get; set; }
        public Course PrerequisiteCourse { get; set; }

        // Navigation
        // public ICollection<Class> Classes { get; set; }
    }
}
