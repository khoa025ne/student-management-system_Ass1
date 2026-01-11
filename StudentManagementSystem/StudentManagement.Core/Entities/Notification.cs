using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagement.Core.Entities
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        // THÊM MỚI: liên kết tới Student
        public int? StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }

        // THÊM MỚI: kiểu thông báo + trạng thái đọc
        public string Type { get; set; }          // "AiAnalysis", "ScoreUpdate", ...
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
