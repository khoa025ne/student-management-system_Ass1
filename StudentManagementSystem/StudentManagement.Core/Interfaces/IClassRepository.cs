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
    public interface IClassRepository
    {
        Task<IEnumerable<Class>> GetBySemesterAsync(int semesterId);
        Task<Class> GetByIdAsync(int id);
        Task AddAsync(Class classEntity);
        Task<bool> CheckRoomAvailabilityAsync(string room, string schedule, int semesterId); // Check trùng phòng
    }
}
