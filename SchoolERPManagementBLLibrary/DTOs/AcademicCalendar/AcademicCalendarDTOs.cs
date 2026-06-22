using System;
using System.Collections.Generic;

namespace SchoolERPManagementBLLibrary.DTOs.AcademicCalendar;

public record CreateCalendarEventDTO(
    DateOnly Date,
    string Description,
    bool IsHoliday,
    int AcademicYearId
);

public record CalendarEventResponseDTO(
    int Id,
    DateOnly Date,
    string Description,
    bool IsHoliday,
    int AcademicYearId
);

public record AcademicCalendarSummaryDTO(
    IReadOnlyList<CalendarEventResponseDTO> Events,
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalDays,
    int WeekendDays,
    int HolidayDays,
    int WorkingDays
);
