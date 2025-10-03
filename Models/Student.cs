namespace MyFirstSKApp.Models;

/// <summary>
/// 學生資料模型
/// </summary>
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty; // 年級
    public DateTime EnrollmentDate { get; set; }
    public List<Enrollment> Enrollments { get; set; } = new();
    public List<Homework> Homeworks { get; set; } = new();
}