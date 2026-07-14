using System;
using System.Collections.Generic;

namespace SchoolERPManagementBLLibrary.DTOs.Student;

public record UpdateStudentDTO(
    string? FirstName, string? LastName, 
    string? Gender, 
    IEnumerable<ParentSelectionDTO>? Parents,
    string? Bloodgroup = null,
    DateOnly? Dateofbirth = null,
    DateOnly? Admissiondate = null
);
