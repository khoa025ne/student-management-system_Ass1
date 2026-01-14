using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Enums;
using StudentManagement.Core.Interfaces;
using StudentManagement.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            .ToListAsync();
    }

    public async Task<Class> GetByIdAsync(int id)
    {
        return await _context.Classes.FindAsync(id);
    }

    public async Task AddAsync(Class cls)
    {
        _context.Classes.Add(cls);
        await _context.SaveChangesAsync();
    }

    // MỚI: check phòng theo kỳ + cặp ngày + slot
    public async Task<bool> CheckRoomAvailabilityAsync(
        string room,
        int semesterId,
        DayOfWeekPair dayOfWeekPair,
        TimeSlot timeSlot)
    {
        return !await _context.Classes.AnyAsync(c =>
            c.SemesterId == semesterId &&
            c.Room == room &&
            c.DayOfWeekPair == dayOfWeekPair &&
            c.TimeSlot == timeSlot
        );
    }
}
