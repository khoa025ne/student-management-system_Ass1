using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Core.DTOs
{
    // DTO để nhận request (Có thể tách ra file riêng)
    public class RegisterDto
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
    }
}
