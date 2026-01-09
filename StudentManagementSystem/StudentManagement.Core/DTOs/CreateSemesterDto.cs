using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Core.DTOs
{
    public class CreateSemesterDto
    {
        public string SemesterName { get; set; } // Ví dụ: Spring 2024
        public string SemesterCode { get; set; } // Ví dụ: SPR24
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
