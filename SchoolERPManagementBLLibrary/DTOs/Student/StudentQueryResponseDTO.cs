using System;

namespace SchoolERPManagementBLLibrary.DTOs.Student;

public record StudentQueryResponseDTO(
    int Id,
    int UserId,
    string RegNo,
    string Name,
    string? ParentName,
    string? ClassName,
    string? Section,
    string? Gender,
    DateOnly? AdmissionDate,
    string? Status,
    string? ProfilePhotoUrl = null,
    string? Bloodgroup = null,
    DateOnly? Dateofbirth = null,
    System.Collections.Generic.IEnumerable<ParentSelectionDTO>? Parents = null,
    int? ClassId = null
);
