using System;
using System.Collections.Generic;

namespace SchoolERPManagementBLLibrary.DTOs.Student;

public record CreateStudentDTO(
    string Email, 
    string Name, 
    int ClassId, 
    int AcademicYearId, 
    IEnumerable<ParentSelectionDTO>? Parents,
    string? Gender = null,
    string? Bloodgroup = null,
    DateOnly? Dateofbirth = null,
    DateOnly? Admissiondate = null
);
