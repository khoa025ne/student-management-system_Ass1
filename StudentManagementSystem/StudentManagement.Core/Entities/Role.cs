using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Core.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } // Admin, Manager, Teacher, Student
        public string Description { get; set; }

        // Navigation Properties
        public virtual ICollection<User> Users { get; set; }
    }
}
