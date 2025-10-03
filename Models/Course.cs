namespace MyFirstSKApp.Models;

/// <summary>
/// 課程資料模型
/// </summary>
public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty; // 科目（數學、英文、物理等）
    public string Description { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MaxStudents { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Enrollment> Enrollments { get; set; } = new();
}