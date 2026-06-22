using System;
using System.Collections.Generic;

namespace SchoolERPManagementBLLibrary.DTOs.Dashboard;

public record TeacherDashboardDTO(
    int TotalStudents,
    int TotalClasses,
    int TotalSubjects,
    int PendingHomeworkCount,
    double StudentAttendanceRate,
    IEnumerable<TeacherTimetableSlotDTO> TodaySchedule,
    IEnumerable<TeacherHomeworkDTO> RecentHomework
);

public record TeacherTimetableSlotDTO(
    int Id,
    string DayOfWeek,
    string StartTime,
    string EndTime,
    string ClassName,
    string Section,
    string SubjectName
);

public record TeacherHomeworkDTO(
    int Id,
    string Title,
    string ClassName,
    string Section,
    string SubjectName,
    string DueDate,
    int SubmissionsCount,
    int TotalStudentsCount
);
