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
using System.Threading.Tasks;

namespace StudentManagement.Infrastructure.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students.Include(s => s.User).ToListAsync();
        }

        public async Task<Student> GetByIdAsync(int id)
        {
            return await _context.Students.Include(s => s.User)
                                 .FirstOrDefaultAsync(s => s.StudentId == id);
        }

        public async Task AddAsync(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
        }
    }
}
