using System.Collections.Generic;

namespace SchoolERPManagementBLLibrary.DTOs.Class;

public record CreateClassDTO(string Classname, string? Section, int? ClassteacherId, int? AcademicyearId, List<int>? SubjectIds);
