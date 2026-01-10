using StudentManagement.Core.Entities;
using StudentManagement.Core.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace StudentManagement.Core.Interfaces
{
    public interface IClassRepository
    {
        Task<IEnumerable<Class>> GetBySemesterAsync(int semesterId);
        Task<Class> GetByIdAsync(int id);
        Task AddAsync(Class cls);

        // SỬA: check trùng phòng theo kỳ + cặp ngày + slot
        Task<bool> CheckRoomAvailabilityAsync(
            string room,
            int semesterId,
            DayOfWeekPair dayOfWeekPair,
            TimeSlot timeSlot);
    }

}
