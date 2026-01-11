using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using StudentManagement.Core.Interfaces;
using StudentManagement.Infrastructure.Data;
using StudentManagement.Infrastructure.Repositories;
using StudentManagement.Infrastructure.Services;
using StudentManagement.Infrastructure.Services;


namespace StudentManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            

            var builder = WebApplication.CreateBuilder(args);
            var geminiApiKey = builder.Configuration["Gemini:ApiKey"];
            // =================================================================
            // 1. CẤU HÌNH SERVICES (DI CONTAINER)
            // =================================================================

            // A. Cấu hình CORS (Cho phép Frontend/Swagger gọi API và xem ảnh)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    b =>
                    {
                        b.AllowAnyOrigin()  // Cho phép mọi nguồn
                         .AllowAnyMethod()  // Cho phép mọi method (GET, POST...)
                         .AllowAnyHeader(); // Cho phép mọi header
                    });
            });

            // B. Cấu hình Database
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    mysqlOptions =>
                    {
                        mysqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    }
                );

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // C. Đăng ký các Services & Repositories
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
            builder.Services.AddScoped<IClassRepository, ClassRepository>();
            builder.Services.AddScoped<EnrollmentService>();
            builder.Services.AddScoped<IGradeService, GradeService>(); // Service Nhập điểm
            builder.Services.AddScoped<EnrollmentService>();

            // D. Các Service cơ bản mặc định
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // AI Geminiiiiiiiii

            builder.Services.AddHttpClient<GeminiService>();
            builder.Services.AddScoped<IAiAnalysisService, AiAnalysisService>();
            builder.Services.AddScoped<IGradeService, GradeService>();



            builder.Services.AddHttpClient<GeminiService>();
            builder.Services.AddScoped<IAiAnalysisService, AiAnalysisService>();


            //mail
            builder.Services.AddScoped<IEmailService, EmailService>();




            // =================================================================
            // 2. BUILD APP & CẤU HÌNH MIDDLEWARE (PIPELINE)
            // =================================================================
            var app = builder.Build();

            // A. Swagger (Môi trường Dev)
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }




            // B. Static Files (QUAN TRỌNG: Để xem ảnh Avatar)
            app.UseStaticFiles();

            // C. CORS (QUAN TRỌNG: Phải đặt trước Authorization)
            app.UseCors("AllowAll");

            // D. Các Middleware khác
            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();

            // =================================================================
            // 3. CHẠY APP
            // =================================================================
            app.Run();
        }
    }
}
