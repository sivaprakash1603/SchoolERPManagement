using System.Collections.Generic;

namespace SchoolERPManagementBLLibrary.DTOs.Timetable;

public record PeriodTimingDTO(
    int PeriodNumber,
    string StartTime,
    string EndTime
);

public record GenerateTimetableRequestDTO(
    List<int> ClassIds,
    int PeriodsPerDay,
    int FreePeriodsPerStaff,
    List<PeriodTimingDTO> Timings
);
