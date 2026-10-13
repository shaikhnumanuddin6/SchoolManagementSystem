
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolManagementSystem.Application.ViewModels.Attendance;

public class AttendanceListViewModel
{
    public DateTime Date { get; set; } = DateTime.Today;

    public int? ClassId { get; set; }

    public int? StudentId { get; set; }

    public string? Status { get; set; }

    public List<AttendanceListItemViewModel> Records { get; set; } = new();

    public List<SelectListItem> Classes { get; set; } = new();

    public List<SelectListItem> Students { get; set; } = new();

    public List<SelectListItem> Statuses { get; set; } = new();

    public int TotalRecords { get; set; }

    public int PresentCount { get; set; }

    public int AbsentCount { get; set; }

    public int LateCount { get; set; }

    public int ExcusedCount { get; set; }

    public double AttendancePercentage { get; set; }
}

public class AttendanceListItemViewModel
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string AdmissionNumber { get; set; } = string.Empty;

    public int? ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string Status { get; set; } = "Present";

    public string? Remarks { get; set; }
}
