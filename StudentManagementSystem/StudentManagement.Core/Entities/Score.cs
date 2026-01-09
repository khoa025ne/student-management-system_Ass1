using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Core.Entities
{
    public class Score
    {
        public int ScoreId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public double ScoreValue { get; set; } // Ví dụ điểm số

        // Navigation properties
        public Student Student { get; set; }
    }
}
