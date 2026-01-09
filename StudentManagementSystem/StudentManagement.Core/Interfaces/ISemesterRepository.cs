using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentManagement.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentManagement.Core.Interfaces
{
    public interface ISemesterRepository
    {
        Task<IEnumerable<Semester>> GetAllAsync();
        Task AddAsync(Semester semester);
        Task SetActiveSemesterAsync(int id); // Logic set kỳ hiện tại
    }
}

