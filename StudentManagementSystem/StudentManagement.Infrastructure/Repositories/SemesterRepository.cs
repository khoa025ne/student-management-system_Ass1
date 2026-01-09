using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using StudentManagement.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentManagement.Infrastructure.Repositories
{
    public class SemesterRepository : ISemesterRepository
    {
        private readonly AppDbContext _context;

        public SemesterRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Semester>> GetAllAsync()
        {
            return await _context.Semesters.ToListAsync();
        }

        public async Task AddAsync(Semester semester)
        {
            _context.Semesters.Add(semester);
            await _context.SaveChangesAsync();
        }

        public async Task SetActiveSemesterAsync(int id)
        {
            // Tắt active tất cả các kỳ khác
            var activeSemesters = await _context.Semesters.Where(s => s.IsActive).ToListAsync();
            foreach (var s in activeSemesters) s.IsActive = false;

            // Bật active kỳ được chọn
            var target = await _context.Semesters.FindAsync(id);
            if (target != null) target.IsActive = true;

            await _context.SaveChangesAsync();
        }
    }
}
