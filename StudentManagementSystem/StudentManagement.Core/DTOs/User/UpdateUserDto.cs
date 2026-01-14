using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Core.DTOs.User
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; }
    }
}
