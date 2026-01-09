using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudentManagement.Core.Entities
{
    public class Semester
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } // Spring 2024
        public string SemesterCode { get; set; } // SPR24
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } // Kỳ hiện tại đang học

        // Navigation
        // public ICollection<Class> Classes { get; set; } // Uncomment khi có Class
    }
}

