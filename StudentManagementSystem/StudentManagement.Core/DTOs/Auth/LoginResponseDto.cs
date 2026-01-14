using System;

namespace StudentManagement.Core.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserInfoDto User { get; set; }
        public bool MustChangePassword { get; set; }
    }
}
