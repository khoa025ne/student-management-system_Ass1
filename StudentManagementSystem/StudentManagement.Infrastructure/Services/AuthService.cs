using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudentManagement.Core.DTOs.Auth;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using StudentManagement.Core.Validators;

namespace StudentManagement.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            // Tìm user theo email
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng");
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Generate tokens
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Save refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            return new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryMinutes"])),
                User = MapToUserInfoDto(user),
                MustChangePassword = user.MustChangePassword
            };
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            // Check if email exists
            if (await _userRepository.EmailExistsAsync(dto.Email))
            {
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = "Email đã tồn tại trong hệ thống"
                };
            }

            // Validate password
            var (isValid, errors) = PasswordValidator.Validate(dto.Password);
            if (!isValid)
            {
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", errors)
                };
            }

            // Create new user with Student role (RoleId = 4)
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                RoleId = 4, // Student role
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                MustChangePassword = false
            };

            var createdUser = await _userRepository.CreateAsync(user);

            return new RegisterResponseDto
            {
                Success = true,
                Message = "Đăng ký thành công",
                User = MapToUserInfoDto(createdUser)
            };
        }

        public async Task<LoginResponseDto> GoogleLoginAsync(GoogleLoginDto dto)
        {
            try
            {
                // Verify Google token
                Google.Apis.Auth.GoogleJsonWebSignature.Payload payload;
                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(dto.GoogleToken, new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _configuration["Google:ClientId"] }
                    });
                }
                catch (InvalidJwtException ex)
                {
                    throw new UnauthorizedAccessException($"Google token không hợp lệ: {ex.Message}");
                }
                catch (Exception ex)
                {
                    throw new UnauthorizedAccessException($"Lỗi xác thực Google: {ex.Message}");
                }

                if (payload == null)
                {
                    throw new UnauthorizedAccessException("Google token không hợp lệ");
                }

                // Check if user exists with GoogleId
                var user = await _userRepository.GetByGoogleIdAsync(payload.Subject);

                if (user == null)
                {
                    // Check if email exists
                    user = await _userRepository.GetByEmailAsync(payload.Email);

                    if (user == null)
                    {
                        // Create new user
                        user = new User
                        {
                            Email = payload.Email,
                            FullName = payload.Name,
                            GoogleId = payload.Subject,
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Random password
                            RoleId = 4, // Student role
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true,
                            MustChangePassword = false,
                            PhoneNumber = ""
                        };

                        user = await _userRepository.CreateAsync(user);
                    }
                    else
                    {
                        // Link Google account to existing user
                        user.GoogleId = payload.Subject;
                        await _userRepository.UpdateAsync(user);
                    }
                }

                if (!user.IsActive)
                {
                    throw new UnauthorizedAccessException("Tài khoản đã bị khóa");
                }

                // Update last login
                user.LastLogin = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // Generate tokens
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userRepository.UpdateAsync(user);

                return new LoginResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryMinutes"])),
                    User = MapToUserInfoDto(user),
                    MustChangePassword = false
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException($"Đăng nhập Google thất bại: {ex.Message}");
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                throw new Exception("Không tìm thấy người dùng");
            }

            // Verify old password
            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Mật khẩu cũ không đúng");
            }

            // Validate new password
            var (isValid, errors) = PasswordValidator.Validate(dto.NewPassword);
            if (!isValid)
            {
                throw new Exception(string.Join(", ", errors));
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.MustChangePassword = false;
            user.PasswordChangedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> ChangePasswordByEmailAsync(ChangePasswordByEmailDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            
            if (user == null)
            {
                throw new Exception("Không tìm thấy tài khoản với email này");
            }

            // Verify old password
            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Mật khẩu cũ không đúng");
            }

            // Validate new password
            var (isValid, errors) = PasswordValidator.Validate(dto.NewPassword);
            if (!isValid)
            {
                throw new Exception(string.Join(", ", errors));
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.MustChangePassword = false;
            user.PasswordChangedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetAllAsync();
            var foundUser = user.FirstOrDefault(u => u.RefreshToken == refreshToken);

            if (foundUser == null || foundUser.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn");
            }

            var newToken = GenerateJwtToken(foundUser);
            var newRefreshToken = GenerateRefreshToken();

            foundUser.RefreshToken = newRefreshToken;
            foundUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(foundUser);

            return new LoginResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryMinutes"])),
                User = MapToUserInfoDto(foundUser),
                MustChangePassword = foundUser.MustChangePassword
            };
        }

        public async Task<bool> RevokeTokenAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userRepository.UpdateAsync(user);
            
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim("RoleId", user.RoleId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private UserInfoDto MapToUserInfoDto(User user)
        {
            return new UserInfoDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                RoleName = user.Role?.RoleName ?? "Unknown",
                RoleId = user.RoleId,
                IsActive = user.IsActive
            };
        }
    }
}