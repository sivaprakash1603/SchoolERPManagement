using System.Collections.Generic;

namespace SchoolERPManagementBLLibrary.DTOs.Class;

public record UpdateClassDTO(string Classname, string? Section, int? ClassteacherId, int? AcademicyearId, List<int>? SubjectIds);
