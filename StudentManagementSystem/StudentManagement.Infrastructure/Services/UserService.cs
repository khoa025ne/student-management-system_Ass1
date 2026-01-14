using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using StudentManagement.Core.DTOs.User;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using StudentManagement.Core.Validators;

namespace StudentManagement.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IEmailService _emailService;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _emailService = emailService;
        }

        public async Task<UserDto> CreateUserByAdminAsync(CreateUserByAdminDto dto)
        {
            // Check if email exists
            if (await _userRepository.EmailExistsAsync(dto.Email))
            {
                throw new Exception("Email đã tồn tại trong hệ thống");
            }

            // Check if role exists
            if (!await _roleRepository.RoleExistsAsync(dto.RoleId))
            {
                throw new Exception("Role không tồn tại");
            }

            // Generate random password
            var randomPassword = PasswordValidator.GenerateRandomPassword(12);

            // Create user
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(randomPassword),
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                RoleId = dto.RoleId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                MustChangePassword = true // Bắt buộc đổi mật khẩu lần đầu
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // Send email with temporary password
            try
            {
                var emailSubject = "Tài khoản của bạn đã được tạo - Student Management System";
                var emailBody = $@"
                    <h2>Chào {createdUser.FullName},</h2>
                    <p>Tài khoản của bạn đã được tạo thành công trong hệ thống Student Management System.</p>
                    <p><strong>Thông tin đăng nhập:</strong></p>
                    <ul>
                        <li><strong>Email:</strong> {createdUser.Email}</li>
                        <li><strong>Mật khẩu tạm thời:</strong> {randomPassword}</li>
                        <li><strong>Vai trò:</strong> {createdUser.Role.RoleName}</li>
                    </ul>
                    <p><strong style='color: red;'>LƯU Ý QUAN TRỌNG:</strong></p>
                    <ul>
                        <li>Bạn PHẢI đổi mật khẩu ngay sau khi đăng nhập lần đầu</li>
                        <li>Mật khẩu mới phải có:
                            <ul>
                                <li>Ít nhất 8 ký tự</li>
                                <li>Ít nhất 1 chữ hoa (A-Z)</li>
                                <li>Ít nhất 1 chữ thường (a-z)</li>
                                <li>Ít nhất 1 chữ số (0-9)</li>
                                <li>Ít nhất 1 ký tự đặc biệt (!@#$%^&*...)</li>
                            </ul>
                        </li>
                        <li>Không chia sẻ mật khẩu với bất kỳ ai</li>
                    </ul>
                    <p>Vui lòng đăng nhập vào hệ thống và đổi mật khẩu ngay.</p>
                    <p>Trân trọng,<br/>Student Management System Team</p>
                ";

                await _emailService.SendEmailAsync(createdUser.Email, emailSubject, emailBody);
            }
            catch (Exception ex)
            {
                // Log error but don't fail user creation
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }

            return MapToUserDto(createdUser);
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Không tìm thấy người dùng");
            }

            return MapToUserDto(user);
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToUserDto).ToList();
        }

        public async Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Không tìm thấy người dùng");
            }

            // Update fields
            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.IsActive = dto.IsActive;

            var updatedUser = await _userRepository.UpdateAsync(user);
            return MapToUserDto(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Không tìm thấy người dùng");
            }

            // Check if user is admin (prevent deleting admin)
            if (user.RoleId == 1)
            {
                throw new Exception("Không thể xóa tài khoản Admin");
            }

            return await _userRepository.DeleteAsync(userId);
        }

        public async Task<bool> UpdateUserRoleAsync(UpdateUserRoleDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                throw new Exception("Không tìm thấy người dùng");
            }

            // Check if new role exists
            if (!await _roleRepository.RoleExistsAsync(dto.NewRoleId))
            {
                throw new Exception("Role mới không tồn tại");
            }

            // Prevent changing admin role
            if (user.RoleId == 1 && dto.NewRoleId != 1)
            {
                throw new Exception("Không thể thay đổi role của Admin");
            }

            user.RoleId = dto.NewRoleId;
            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<List<UserDto>> GetUsersByRoleAsync(int roleId)
        {
            var users = await _userRepository.GetByRoleIdAsync(roleId);
            return users.Select(MapToUserDto).ToList();
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName ?? "Unknown",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
                MustChangePassword = user.MustChangePassword,
                HasGoogleAccount = !string.IsNullOrEmpty(user.GoogleId)
            };
        }
    }
}
