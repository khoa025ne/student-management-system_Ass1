namespace StudentManagement.Core.DTOs.Auth
{
    public class RegisterResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserInfoDto User { get; set; }
    }
}
