using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Core.DTOs
{
    public class CreateClassDto
    {
        public string ClassName { get; set; } // SE1801
        public int CourseId { get; set; }
        public int SemesterId { get; set; }
        public string Room { get; set; }
        public string Schedule { get; set; } // Mon-Wed-Fri
        public int MaxCapacity { get; set; }
    }
}
