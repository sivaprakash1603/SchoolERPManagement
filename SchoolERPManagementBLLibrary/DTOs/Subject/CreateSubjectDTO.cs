using System.ComponentModel.DataAnnotations;

namespace SchoolERPManagementBLLibrary.DTOs.Subject;

public record CreateSubjectDTO(
    [Required] [MaxLength(100)] string SubjectName
);
