using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.EntityFrameworkCore;
using MyFirstSKApp.Data;
using MyFirstSKApp.Models;
using System.Text.Json;

namespace MyFirstSKApp.Plugins;

/// <summary>
/// 學生相關功能的 Plugin
/// </summary>
public class StudentPlugin
{
    private readonly TutorialSchoolDbContext _dbContext;

    public StudentPlugin(TutorialSchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [KernelFunction("GetStudentHomework")]
    [Description("取得學生的作業資料，包括成績和老師回饋")]
    public async Task<string> GetStudentHomeworkAsync(
        [Description("學生姓名")] string studentName,
        [Description("課程名稱（可選）")] string? courseName = null)
    {
        var query = _dbContext.Homeworks
            .Include(h => h.Student)
            .Include(h => h.Course)
            .Where(h => h.Student.Name.Contains(studentName));

        if (!string.IsNullOrEmpty(courseName))
        {
            query = query.Where(h => h.Course.Name.Contains(courseName));
        }

        var homeworks = await query
            .OrderByDescending(h => h.AssignedDate)
            .Take(10)
            .ToListAsync();

        if (!homeworks.Any())
        {
            return $"找不到學生 {studentName} 的作業記錄";
        }

        var result = homeworks.Select(h => new
        {
            學生 = h.Student.Name,
            課程 = h.Course.Name,
            作業標題 = h.Title,
            作業描述 = h.Description,
            指派日期 = h.AssignedDate.ToString("yyyy/MM/dd"),
            截止日期 = h.DueDate.ToString("yyyy/MM/dd"),
            繳交日期 = h.SubmittedDate?.ToString("yyyy/MM/dd") ?? "未繳交",
            成績 = h.Score?.ToString() ?? "未評分",
            老師回饋 = h.Feedback ?? "無回饋",
            狀態 = h.Status.ToString(),
            學生作答內容 = h.SubmittedContent ?? "未繳交"
        }).ToList();

        return JsonSerializer.Serialize(result, new JsonSerializerOptions 
        { 
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true 
        });
    }

    [KernelFunction("GetStudentProfile")]
    [Description("取得學生的基本資料和選課狀況")]
    public async Task<string> GetStudentProfileAsync(
        [Description("學生姓名")] string studentName)
    {
        var student = await _dbContext.Students
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.Name.Contains(studentName));

        if (student == null)
        {
            return $"找不到學生 {studentName}";
        }

        var result = new
        {
            學生姓名 = student.Name,
            電子郵件 = student.Email,
            年級 = student.Grade,
            入學日期 = student.EnrollmentDate.ToString("yyyy/MM/dd"),
            選修課程 = student.Enrollments.Select(e => new
            {
                課程名稱 = e.Course.Name,
                科目 = e.Course.Subject,
                老師 = e.Course.TeacherName,
                選課日期 = e.EnrollmentDate.ToString("yyyy/MM/dd"),
                狀態 = e.Status,
                期末成績 = e.FinalGrade?.ToString() ?? "未評分"
            }).ToList()
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions 
        { 
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true 
        });
    }

    [KernelFunction("SubmitHomeworkFeedback")]
    [Description("為學生作業提供 AI 協助回饋")]
    public async Task<string> SubmitHomeworkFeedbackAsync(
        [Description("學生姓名")] string studentName,
        [Description("作業標題")] string homeworkTitle,
        [Description("AI 建議回饋")] string aiFeedback)
    {
        var homework = await _dbContext.Homeworks
            .Include(h => h.Student)
            .FirstOrDefaultAsync(h => 
                h.Student.Name.Contains(studentName) && 
                h.Title.Contains(homeworkTitle));

        if (homework == null)
        {
            return $"找不到學生 {studentName} 的作業 {homeworkTitle}";
        }

        // 將 AI 回饋附加到現有回饋中
        var timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        var newFeedback = $"[AI 助理 {timestamp}]\n{aiFeedback}";
        
        if (!string.IsNullOrEmpty(homework.Feedback))
        {
            homework.Feedback += $"\n\n{newFeedback}";
        }
        else
        {
            homework.Feedback = newFeedback;
        }

        await _dbContext.SaveChangesAsync();

        return $"已為學生 {studentName} 的作業 {homeworkTitle} 添加 AI 回饋";
    }
}