using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using StudentManagement.Core.Interfaces;
using StudentManagement.Infrastructure.Data; // Đảm bảo namespace này khớp với nơi bạn đặt AppDbContext
using StudentManagement.Infrastructure.Repositories;
using StudentManagement.Infrastructure.Services;
namespace StudentManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // ...
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            // ...
            builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
            // Thêm dòng này cùng chỗ với các Services khác
            builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
            // Thêm vào Program.cs
            builder.Services.AddScoped<IClassRepository, ClassRepository>();
            builder.Services.AddScoped<EnrollmentService>();



            // =================================================================
            // 1. CẤU HÌNH DATABASE (MYSQL - POMELO)
            // =================================================================

            // Lấy chuỗi kết nối từ appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Kiểm tra nếu connection string chưa được cấu hình
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            // Đăng ký AppDbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString), // Tự động phát hiện version MySQL
                    mysqlOptions =>
                    {
                        // Cấu hình thêm nếu cần (ví dụ: tự động retry khi mất kết nối)
                        mysqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    }
                );

                // Log SQL ra console khi ở môi trường Development để dễ debug
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging(); // Hiện tham số query
                    options.EnableDetailedErrors();
                }
            });

            // =================================================================

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
