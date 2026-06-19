using SchoolERPManagementBLLibrary.DTOs.Report.Query;

namespace SchoolERPManagementBLLibrary.DTOs.Parent;

public class ParentQueryRequest : PagedQueryRequest
{
    public string? SearchQuery { get; set; }
    public string? Status { get; set; }
}
