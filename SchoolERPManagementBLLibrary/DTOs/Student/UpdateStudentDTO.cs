using System;
using System.Collections.Generic;

namespace SchoolERPManagementBLLibrary.DTOs.Student;

public record UpdateStudentDTO(
    string? Name, 
    string? Gender, 
    IEnumerable<ParentSelectionDTO>? Parents,
    string? Bloodgroup = null,
    DateOnly? Dateofbirth = null,
    DateOnly? Admissiondate = null
);
