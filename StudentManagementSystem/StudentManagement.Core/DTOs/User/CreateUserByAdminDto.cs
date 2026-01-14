using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Core.DTOs.User
{
    public class CreateUserByAdminDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Role là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Role ID phải lớn hơn 0")]
        public int RoleId { get; set; }
    }
}
