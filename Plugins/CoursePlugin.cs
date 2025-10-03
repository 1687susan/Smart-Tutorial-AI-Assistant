using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.EntityFrameworkCore;
using MyFirstSKApp.Data;
using MyFirstSKApp.Models;
using System.Text.Json;

namespace MyFirstSKApp.Plugins;

/// <summary>
/// 課程相關功能的 Plugin
/// </summary>
public class CoursePlugin
{
    private readonly TutorialSchoolDbContext _dbContext;

    public CoursePlugin(TutorialSchoolDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [KernelFunction("GetCourseList")]
    [Description("取得課程列表，可依科目篩選")]
    public async Task<string> GetCourseListAsync(
        [Description("科目名稱（可選，如：數學、英文、物理）")] string? subject = null)
    {
        var query = _dbContext.Courses.AsQueryable();

        if (!string.IsNullOrEmpty(subject))
        {
            query = query.Where(c => c.Subject.Contains(subject));
        }

        var courses = await query
            .Include(c => c.Enrollments)
            .OrderBy(c => c.StartDate)
            .ToListAsync();

        if (!courses.Any())
        {
            return subject == null ? "目前沒有開設課程" : $"目前沒有開設 {subject} 相關課程";
        }

        var result = courses.Select(c => new
        {
            課程編號 = c.Id,
            課程名稱 = c.Name,
            科目 = c.Subject,
            課程描述 = c.Description,
            授課老師 = c.TeacherName,
            學費 = c.Price,
            招生人數上限 = c.MaxStudents,
            目前報名人數 = c.Enrollments.Count,
            開課日期 = c.StartDate.ToString("yyyy/MM/dd"),
            結束日期 = c.EndDate.ToString("yyyy/MM/dd"),
            是否可報名 = c.Enrollments.Count < c.MaxStudents ? "可報名" : "已額滿"
        }).ToList();

        return JsonSerializer.Serialize(result, new JsonSerializerOptions 
        { 
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true 
        });
    }

    [KernelFunction("GetCourseDetails")]
    [Description("取得特定課程的詳細資訊，包括學生名單")]
    public async Task<string> GetCourseDetailsAsync(
        [Description("課程名稱或課程編號")] string courseIdentifier)
    {
        Course? course = null;

        // 嘗試以課程編號搜尋
        if (int.TryParse(courseIdentifier, out int courseId))
        {
            course = await _dbContext.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }

        // 如果沒找到，嘗試以課程名稱搜尋
        if (course == null)
        {
            course = await _dbContext.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Name.Contains(courseIdentifier));
        }

        if (course == null)
        {
            return $"找不到課程：{courseIdentifier}";
        }

        var result = new
        {
            課程資訊 = new
            {
                課程編號 = course.Id,
                課程名稱 = course.Name,
                科目 = course.Subject,
                課程描述 = course.Description,
                授課老師 = course.TeacherName,
                學費 = course.Price,
                招生人數上限 = course.MaxStudents,
                開課日期 = course.StartDate.ToString("yyyy/MM/dd"),
                結束日期 = course.EndDate.ToString("yyyy/MM/dd")
            },
            學生名單 = course.Enrollments
                .Where(e => e.Status == "Active")
                .Select(e => new
                {
                    學生姓名 = e.Student.Name,
                    年級 = e.Student.Grade,
                    選課日期 = e.EnrollmentDate.ToString("yyyy/MM/dd"),
                    期末成績 = e.FinalGrade?.ToString() ?? "未評分"
                }).ToList(),
            統計資訊 = new
            {
                目前報名人數 = course.Enrollments.Count(e => e.Status == "Active"),
                剩餘名額 = course.MaxStudents - course.Enrollments.Count(e => e.Status == "Active")
            }
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions 
        { 
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true 
        });
    }

    [KernelFunction("RecommendCourses")]
    [Description("根據學生年級和興趣推薦適合的課程")]
    public async Task<string> RecommendCoursesAsync(
        [Description("學生年級（如：國一、國二、國三、高一、高二、高三）")] string grade,
        [Description("感興趣的科目（可選）")] string? preferredSubject = null)
    {
        var query = _dbContext.Courses
            .Include(c => c.Enrollments)
            .AsQueryable();

        // 根據年級篩選適合的課程
        var gradeKeywords = GetGradeKeywords(grade);
        if (gradeKeywords.Any())
        {
            query = query.Where(c => gradeKeywords.Any(keyword => c.Name.Contains(keyword)));
        }

        // 如果指定科目偏好
        if (!string.IsNullOrEmpty(preferredSubject))
        {
            query = query.Where(c => c.Subject.Contains(preferredSubject));
        }

        var courses = await query
            .Where(c => c.StartDate <= DateTime.Now.AddMonths(1)) // 一個月內開課
            .Where(c => c.Enrollments.Count(e => e.Status == "Active") < c.MaxStudents) // 還有名額
            .OrderBy(c => c.StartDate)
            .ToListAsync();

        if (!courses.Any())
        {
            return $"目前沒有適合 {grade} 學生的課程推薦";
        }

        var result = new
        {
            推薦原因 = $"為 {grade} 學生推薦以下課程",
            推薦課程 = courses.Select(c => new
            {
                課程名稱 = c.Name,
                科目 = c.Subject,
                授課老師 = c.TeacherName,
                學費 = c.Price,
                開課日期 = c.StartDate.ToString("yyyy/MM/dd"),
                剩餘名額 = c.MaxStudents - c.Enrollments.Count(e => e.Status == "Active"),
                推薦指數 = CalculateRecommendationScore(c, grade, preferredSubject)
            }).OrderByDescending(c => c.推薦指數).ToList()
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions 
        { 
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true 
        });
    }

    private List<string> GetGradeKeywords(string grade)
    {
        return grade.ToLower() switch
        {
            var g when g.Contains("國一") || g.Contains("七年級") => new List<string> { "國一", "七年級", "國中基礎" },
            var g when g.Contains("國二") || g.Contains("八年級") => new List<string> { "國二", "八年級", "國中進階" },
            var g when g.Contains("國三") || g.Contains("九年級") => new List<string> { "國三", "九年級", "會考", "總復習" },
            var g when g.Contains("高一") => new List<string> { "高一", "高中基礎" },
            var g when g.Contains("高二") => new List<string> { "高二", "高中進階" },
            var g when g.Contains("高三") => new List<string> { "高三", "學測", "指考", "總復習" },
            _ => new List<string>()
        };
    }

    private int CalculateRecommendationScore(Course course, string grade, string? preferredSubject)
    {
        int score = 50; // 基礎分數

        // 課程名稱符合年級 +30 分
        var gradeKeywords = GetGradeKeywords(grade);
        if (gradeKeywords.Any(keyword => course.Name.Contains(keyword)))
        {
            score += 30;
        }

        // 科目偏好符合 +20 分
        if (!string.IsNullOrEmpty(preferredSubject) && course.Subject.Contains(preferredSubject))
        {
            score += 20;
        }

        // 剩餘名額多 +10 分
        var availableSpots = course.MaxStudents - course.Enrollments.Count(e => e.Status == "Active");
        if (availableSpots > course.MaxStudents * 0.5)
        {
            score += 10;
        }

        // 即將開課 +5 分
        if (course.StartDate <= DateTime.Now.AddDays(7))
        {
            score += 5;
        }

        return score;
    }
}