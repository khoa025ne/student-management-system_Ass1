using StudentManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace StudentManagement.Core.Entities
{
    
public class Student
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } // STU202300145
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string ClassCode { get; set; } // SE1801
    public decimal OverallGPA { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Major { get; set; } // Ví dụ: "SE", "IA"
    public string? AvatarUrl { get; set; } // "/uploads/sv123.jpg"


        // Navigation Properties
        public int UserId { get; set; }
    public virtual User User { get; set; }
    public virtual ICollection<Enrollment> Enrollments { get; set; }
    public virtual ICollection<Score> Scores { get; set; }
}
}