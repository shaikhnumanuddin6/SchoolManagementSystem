
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolManagementSystem.Application.ViewModels.Timetable;

public class TimetableListViewModel
{
    public int? ClassId { get; set; }

    public int? DayOfWeek { get; set; }

    public List<SelectListItem> Classes { get; set; } = new();

    public List<TimetableItemViewModel> Timetables { get; set; } = new();
}

public class TimetableItemViewModel
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string SubjectName { get; set; } = string.Empty;

    public string TeacherName { get; set; } = string.Empty;

    public int DayOfWeek { get; set; }

    public string DayName { get; set; } = string.Empty;

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string? RoomNumber { get; set; }
}
