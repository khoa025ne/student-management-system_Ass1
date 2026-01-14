using System.Collections.Generic;
using System.Threading.Tasks;
using StudentManagement.Core.Entities;

namespace StudentManagement.Core.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role> GetByIdAsync(int roleId);
        Task<List<Role>> GetAllAsync();
        Task<bool> RoleExistsAsync(int roleId);
    }
}
