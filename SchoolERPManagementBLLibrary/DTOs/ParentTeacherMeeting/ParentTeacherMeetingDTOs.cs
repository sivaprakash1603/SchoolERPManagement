using System;
using System.Collections.Generic;

namespace SchoolERPManagementBLLibrary.DTOs.ParentTeacherMeeting;

public record PtmMeetingResponseDTO(
    int Id,
    int? AcademicCalendarId,
    DateOnly EventDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Description,
    bool IsActive
);

public record PtmSlotResponseDTO(
    int Id,
    int MeetingId,
    int TeacherId,
    string TeacherName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Status,
    int? ParentId,
    string? ParentName,
    int? StudentId,
    string? StudentName
);

public record BookSlotRequestDTO(
    int SlotId,
    int StudentId
);
