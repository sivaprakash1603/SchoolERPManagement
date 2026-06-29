using System.Collections.Generic;
using SchoolERPManagementBLLibrary.DTOs.Subject;

namespace SchoolERPManagementBLLibrary.DTOs.Class;

public record ClassResponseDTO(
    int Id, 
    string Classname, 
    string? Section, 
    int? ClassteacherId, 
    int? AcademicyearId, 
    int StudentCount = 0,
    List<SubjectResponseDTO>? Subjects = null
);
