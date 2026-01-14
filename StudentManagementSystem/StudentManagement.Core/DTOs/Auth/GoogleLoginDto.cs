using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Core.DTOs.Auth
{
    public class GoogleLoginDto
    {
        [Required(ErrorMessage = "Google token là bắt buộc")]
        public string GoogleToken { get; set; }
    }
}
