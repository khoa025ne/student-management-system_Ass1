using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using StudentManagement.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Infrastructure.Repositories
{
    public class ClassRepository : IClassRepository
    {
        private readonly AppDbContext _context;

        public ClassRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Class>> GetBySemesterAsync(int semesterId)
        {
            return await _context.Classes
                .Where(c => c.SemesterId == semesterId)
                .Include(c => c.Course) // Load kèm tên môn học
                .ToListAsync();
        }

        public async Task<Class> GetByIdAsync(int id)
        {
            return await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ClassId == id);
        }

        public async Task AddAsync(Class classEntity)
        {
            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync();
        }

        // Logic check trùng lịch đơn giản: Cùng phòng + Cùng ca + Cùng kỳ = Trùng
        public async Task<bool> CheckRoomAvailabilityAsync(string room, string schedule, int semesterId)
        {
            return !await _context.Classes.AnyAsync(c =>
                c.SemesterId == semesterId &&
                c.Room == room &&
                c.Schedule == schedule);
        }
    }
}
