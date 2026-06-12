namespace SchoolERPManagementBLLibrary.DTOs.Report.Query;

public class AssetQueryRequest : PagedQueryRequest
{
    public string? Status { get; set; }
    public int? AssetTypeId { get; set; }
}
