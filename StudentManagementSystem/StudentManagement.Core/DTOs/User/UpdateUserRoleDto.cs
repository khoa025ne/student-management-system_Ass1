using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Core.DTOs.User
{
    public class UpdateUserRoleDto
    {
        [Required(ErrorMessage = "User ID là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID phải lớn hơn 0")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Role ID mới là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Role ID phải lớn hơn 0")]
        public int NewRoleId { get; set; }
    }
}
