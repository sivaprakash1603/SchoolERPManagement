using SchoolERPManagementBLLibrary.DTOs.Report.Query;

namespace SchoolERPManagementBLLibrary.DTOs.Student;

public class StudentQueryRequest : PagedQueryRequest
{
    public string? SearchQuery { get; set; }
    public int? ClassId { get; set; }
    public int? AcademicYearId { get; set; }
    public string? Gender { get; set; }
    public string? Status { get; set; }
}
