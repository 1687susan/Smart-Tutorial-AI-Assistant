using Microsoft.EntityFrameworkCore;
using MyFirstSKApp.Models;

namespace MyFirstSKApp.Data;

/// <summary>
/// 補習班資料庫上下文
/// </summary>
public class TutorialSchoolDbContext : DbContext
{
    public TutorialSchoolDbContext(DbContextOptions<TutorialSchoolDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Homework> Homeworks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 配置 Student-Enrollment 關係
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId);

        // 配置 Course-Enrollment 關係
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId);

        // 配置 Student-Homework 關係
        modelBuilder.Entity<Homework>()
            .HasOne(h => h.Student)
            .WithMany(s => s.Homeworks)
            .HasForeignKey(h => h.StudentId);

        // 配置 Course-Homework 關係
        modelBuilder.Entity<Homework>()
            .HasOne(h => h.Course)
            .WithMany()
            .HasForeignKey(h => h.CourseId);

        // 種子資料
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // 學生資料
        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, Name = "張小明", Email = "ming@example.com", Grade = "國三", EnrollmentDate = DateTime.Now.AddMonths(-3) },
            new Student { Id = 2, Name = "李小華", Email = "hua@example.com", Grade = "高一", EnrollmentDate = DateTime.Now.AddMonths(-2) },
            new Student { Id = 3, Name = "王小美", Email = "mei@example.com", Grade = "國二", EnrollmentDate = DateTime.Now.AddMonths(-1) }
        );

        // 課程資料
        modelBuilder.Entity<Course>().HasData(
            new Course { Id = 1, Name = "國三數學總復習", Subject = "數學", Description = "國三會考數學總復習", TeacherName = "陳老師", Price = 12000, MaxStudents = 20, StartDate = DateTime.Now.AddMonths(-2), EndDate = DateTime.Now.AddMonths(2) },
            new Course { Id = 2, Name = "高一英文", Subject = "英文", Description = "高一英文基礎課程", TeacherName = "林老師", Price = 10000, MaxStudents = 25, StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now.AddMonths(3) },
            new Course { Id = 3, Name = "國二物理", Subject = "物理", Description = "國二物理觀念建立", TeacherName = "黃老師", Price = 8000, MaxStudents = 15, StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(4) }
        );

        // 選課記錄
        modelBuilder.Entity<Enrollment>().HasData(
            new Enrollment { Id = 1, StudentId = 1, CourseId = 1, EnrollmentDate = DateTime.Now.AddMonths(-2), Status = "Active" },
            new Enrollment { Id = 2, StudentId = 2, CourseId = 2, EnrollmentDate = DateTime.Now.AddMonths(-1), Status = "Active" },
            new Enrollment { Id = 3, StudentId = 3, CourseId = 3, EnrollmentDate = DateTime.Now, Status = "Active" }
        );

        // 作業資料
        modelBuilder.Entity<Homework>().HasData(
            new Homework { Id = 1, StudentId = 1, CourseId = 1, Title = "二次函數練習", Description = "完成課本 P.45-50 題目", AssignedDate = DateTime.Now.AddDays(-7), DueDate = DateTime.Now.AddDays(-1), SubmittedDate = DateTime.Now.AddDays(-2), Score = 85, Feedback = "計算正確，但要注意圖形標示", Status = HomeworkStatus.Graded },
            new Homework { Id = 2, StudentId = 2, CourseId = 2, Title = "英文閱讀測驗", Description = "閱讀文章並回答問題", AssignedDate = DateTime.Now.AddDays(-5), DueDate = DateTime.Now.AddDays(2), Status = HomeworkStatus.Assigned },
            new Homework { Id = 3, StudentId = 1, CourseId = 1, Title = "三角函數應用", Description = "實際應用題練習", AssignedDate = DateTime.Now.AddDays(-3), DueDate = DateTime.Now.AddDays(1), SubmittedContent = "已完成前 5 題，第 6 題需要協助", SubmittedDate = DateTime.Now.AddDays(-1), Status = HomeworkStatus.Submitted }
        );
    }
}