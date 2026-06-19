using System.Collections.Generic;

namespace SchoolERPManagementBLLibrary.DTOs.Student;

public record BulkEnrollStudentsDTO(List<int> StudentIds, int ClassId, int AcademicYearId);
