namespace MyFirstSKApp.Models;

/// <summary>
/// 作業資料模型
/// </summary>
public class Homework
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SubmittedContent { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
    public HomeworkStatus Status { get; set; } = HomeworkStatus.Assigned;
}

public enum HomeworkStatus
{
    Assigned,    // 已指派
    Submitted,   // 已繳交
    Graded,      // 已評分
    Late         // 逾期
}