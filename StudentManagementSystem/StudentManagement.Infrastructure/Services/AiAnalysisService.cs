using StudentManagement.Core.Entities;
using StudentManagement.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public interface IAiAnalysisService
{
    Task GenerateAndSaveAnalysisAsync(int studentId);
}

public class AiAnalysisService : IAiAnalysisService
{
    private readonly AppDbContext _context;
    private readonly GeminiService _gemini; // service gọi Gemini

    public AiAnalysisService(AppDbContext context, GeminiService gemini)
    {
        _context = context;
        _gemini = gemini;
    }

    public async Task GenerateAndSaveAnalysisAsync(int studentId)
    {
        var student = await _context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Class)
                    .ThenInclude(c => c.Course)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

        if (student == null) return;

        var completed = student.Enrollments
            .Where(e => e.TotalScore != null)
            .ToList();

        var obj = new
        {
            studentId = student.StudentCode,
            overallGPA = student.OverallGPA,
            completedCourses = completed.Select(e => new
            {
                courseName = e.Class.Course.CourseName,
                gpa = e.TotalScore,
                grade = e.Grade,
                credits = e.Class.Course.Credits
            }).ToList()
        };

        var payloadJson = JsonSerializer.Serialize(obj);

        var prompt = $@"
                Bạn là cố vấn học tập hãy nghiên cứu về giáo trình của 
                đại học FPT (Khu vực TP. Hồ Chí Minh để nắm được 
                nội dung môn học cho chuyên nghành tương ứng và đưa ra
                 lời khuyên cũng như những tài liệu liên quan Ví Dụ: 
                link youtube về những nội dung liên quan tới môn học,
                 các tài liệu tham khảo liên quan tới môn học.
                ). Dữ liệu:
                {payloadJson}

                Hãy trả về JSON:
                {{
                  ""strongSubjects"": [""mon1"", ""mon2""],
                  ""weakSubjects"": [""mon3""],
                  ""recommendations"": ""Khuyến nghị tối đa 200 từ""
                }}";

        string aiText;
        try
        {
            aiText = await _gemini.GenerateAnalysisAsync(prompt);
        }
        catch
        {
            // fallback đơn giản
            var strong = completed
                .Where(e => e.Grade == "A" || e.Grade == "B")
                .Select(e => e.Class.Course.CourseName)
                .ToList();
            var weak = completed
                .Where(e => e.Grade == "D" || e.Grade == "F")
                .Select(e => e.Class.Course.CourseName)
                .ToList();

            var fallback = new
            {
                strongSubjects = strong,
                weakSubjects = weak,
                recommendations = "Tập trung củng cố các môn điểm D/F, ôn lại lý thuyết và làm thêm bài tập."
            };

            aiText = JsonSerializer.Serialize(fallback);
        }

        // parse JSON từ AI
        using var doc = JsonDocument.Parse(aiText);
        var root = doc.RootElement;

        var strongJson = root.GetProperty("strongSubjects").GetRawText();
        var weakJson = root.GetProperty("weakSubjects").GetRawText();
        var rec = root.GetProperty("recommendations").GetString();

        var analysis = new AcademicAnalysis
        {
            StudentId = studentId,
            OverallGPA = student.OverallGPA,
            StrongSubjectsJson = strongJson,
            WeakSubjectsJson = weakJson,
            Recommendations = rec,
            AiModelUsed = "Gemini"
        };

        _context.AcademicAnalyses.Add(analysis);

        // tạo notification
        _context.Notifications.Add(new Notification
        {
            StudentId = studentId,
            Title = "Phân tích học tập mới",
            Message = "Hệ thống AI vừa phân tích kết quả học tập của bạn.",
            Type = "AiAnalysis"
        });

        await _context.SaveChangesAsync();
    }
}

