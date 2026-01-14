using System.Collections.Generic;
using System.Threading.Tasks;
using StudentManagement.Core.Entities;

namespace StudentManagement.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(int userId);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByGoogleIdAsync(string googleId);
        Task<bool> EmailExistsAsync(string email);
        Task<List<User>> GetAllAsync();
        Task<List<User>> GetByRoleIdAsync(int roleId);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int userId);
        Task<bool> SaveChangesAsync();
    }
}
