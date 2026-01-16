using System.Collections.Generic;
using System.Threading.Tasks;
using StudentManagement.Core.DTOs.User;

namespace StudentManagement.Core.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> CreateUserByAdminAsync(CreateUserByAdminDto dto);
        Task<UserDto> GetUserByIdAsync(int userId);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto dto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> UpdateUserRoleAsync(UpdateUserRoleDto dto);
        Task<List<UserDto>> GetUsersByRoleAsync(int roleId);
        Task<UserDto> UpdateAvatarAsync(int userId, string avatarUrl);
    }
}
