namespace SchoolERPManagementBLLibrary.DTOs.Asset;

public class AssetStatsDTO
{
    public int TotalAssets { get; set; }
    public int ActiveAssets { get; set; }
    public int UnderRepairAssets { get; set; }
    public int BrokenAssets { get; set; }
}
