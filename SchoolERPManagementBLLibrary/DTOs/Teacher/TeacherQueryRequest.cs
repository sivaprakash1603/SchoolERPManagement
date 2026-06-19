using SchoolERPManagementBLLibrary.DTOs.Report.Query;

namespace SchoolERPManagementBLLibrary.DTOs.Teacher;

public class TeacherQueryRequest : PagedQueryRequest
{
    public string? SearchQuery { get; set; }
    public string? Status { get; set; }
}
