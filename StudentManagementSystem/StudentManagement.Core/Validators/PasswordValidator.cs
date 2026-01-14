using System;
using System.Collections.Generic;
using System.Linq;

namespace StudentManagement.Core.Validators
{
    public class PasswordValidator
    {
        /// <summary>
        /// Validate mật khẩu theo yêu cầu bảo mật
        /// </summary>
        public static (bool IsValid, List<string> Errors) Validate(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Mật khẩu không được để trống");
                return (false, errors);
            }

            if (password.Length < 8)
                errors.Add("Mật khẩu phải có ít nhất 8 ký tự");

            if (!password.Any(char.IsUpper))
                errors.Add("Mật khẩu phải có ít nhất 1 chữ hoa (A-Z)");

            if (!password.Any(char.IsLower))
                errors.Add("Mật khẩu phải có ít nhất 1 chữ thường (a-z)");

            if (!password.Any(char.IsDigit))
                errors.Add("Mật khẩu phải có ít nhất 1 chữ số (0-9)");

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                errors.Add("Mật khẩu phải có ít nhất 1 ký tự đặc biệt (!@#$%^&*...)");

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Tạo mật khẩu ngẫu nhiên đủ điều kiện
        /// </summary>
        public static string GenerateRandomPassword(int length = 12)
        {
            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
            const string digitChars = "0123456789";
            const string specialChars = "!@#$%^&*";

            var random = new Random();

            // SỬA LỖI TẠI ĐÂY: List<char> thay vì Listhar
            var passwordChars = new List<char>
            {
                upperChars[random.Next(upperChars.Length)],
                lowerChars[random.Next(lowerChars.Length)],
                digitChars[random.Next(digitChars.Length)],
                specialChars[random.Next(specialChars.Length)]
            };

            var allChars = upperChars + lowerChars + digitChars + specialChars;
            for (int i = 4; i < length; i++)
            {
                passwordChars.Add(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle và chuyển thành chuỗi
            return new string(passwordChars.OrderBy(x => random.Next()).ToArray());
        }
    }
}