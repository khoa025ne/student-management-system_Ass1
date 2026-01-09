using Microsoft.EntityFrameworkCore; // QUAN TRỌNG: Để dùng DbContext, DbSet
using StudentManagement.Core.Entities; // QUAN TRỌNG: Để dùng User, Student...

namespace StudentManagement.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Khai báo các bảng
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<Semester> Semesters { get; set; }

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Cấu hình bảng User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Email).IsUnique();
            });


            // Cấu hình bảng Student
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.StudentId);
                entity.HasIndex(e => e.StudentCode).IsUnique();
                entity.Property(e => e.OverallGPA).HasColumnType("decimal(3,2)");
            });


            // Seed Data
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin", Description = "Quản trị hệ thống" },
                new Role { RoleId = 2, RoleName = "Manager", Description = "Giáo vụ" },
                new Role { RoleId = 3, RoleName = "Teacher", Description = "Giảng viên" },
                new Role { RoleId = 4, RoleName = "Student", Description = "Sinh viên" }
            );


            modelBuilder.Entity<Semester>(entity =>
            {
                entity.HasKey(e => e.SemesterId);
                entity.HasIndex(e => e.SemesterCode).IsUnique();
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseId);
                entity.HasIndex(e => e.CourseCode).IsUnique();

                // Cấu hình Tiên quyết: Một môn có thể có 1 môn tiên quyết
                entity.HasOne(c => c.PrerequisiteCourse)
                      .WithMany()
                      .HasForeignKey(c => c.PrerequisiteCourseId)
                      .OnDelete(DeleteBehavior.Restrict); // Xóa môn cha không xóa môn con
            });
            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasOne(c => c.Course)
                      .WithMany()
                      .HasForeignKey(c => c.CourseId);

                entity.HasOne(c => c.Semester)
                      .WithMany()
                      .HasForeignKey(c => c.SemesterId);
            });

        }
    }
}
