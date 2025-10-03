namespace MyFirstSKApp.Models;

/// <summary>
/// 學生選課記錄
/// </summary>
public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public DateTime EnrollmentDate { get; set; }
    public decimal? FinalGrade { get; set; }
    public string Status { get; set; } = "Active"; // Active, Completed, Dropped
}