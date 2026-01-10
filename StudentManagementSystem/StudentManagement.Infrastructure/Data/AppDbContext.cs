using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Enums;
using StudentManagement.Core.Enums;


namespace StudentManagement.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<AcademicAnalysis> AcademicAnalyses { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Student
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.StudentId);
                entity.HasIndex(e => e.StudentCode).IsUnique();
                entity.Property(e => e.OverallGPA).HasColumnType("decimal(3,2)");
            });

            // Role seed
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin", Description = "Quản trị hệ thống" },
                new Role { RoleId = 2, RoleName = "Manager", Description = "Giáo vụ" },
                new Role { RoleId = 3, RoleName = "Teacher", Description = "Giảng viên" },
                new Role { RoleId = 4, RoleName = "Student", Description = "Sinh viên" }
            );

            // Semester
            modelBuilder.Entity<Semester>(entity =>
            {
                entity.HasKey(e => e.SemesterId);
                entity.HasIndex(e => e.SemesterCode).IsUnique();
            });

            // Course
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseId);
                entity.HasIndex(e => e.CourseCode).IsUnique();

                entity.HasOne(c => c.PrerequisiteCourse)
                      .WithMany()
                      .HasForeignKey(c => c.PrerequisiteCourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Class
            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasKey(e => e.ClassId);

                entity.HasOne(c => c.Course)
                      .WithMany()
                      .HasForeignKey(c => c.CourseId);

                entity.HasOne(c => c.Semester)
                      .WithMany()
                      .HasForeignKey(c => c.SemesterId);

                // MAPPING ENUM → INT
                entity.Property(c => c.DayOfWeekPair)
                      .HasConversion<int>();

                entity.Property(c => c.TimeSlot)
                      .HasConversion<int>();
            });
        }
    }
}
