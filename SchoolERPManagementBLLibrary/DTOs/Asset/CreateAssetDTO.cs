namespace SchoolERPManagementBLLibrary.DTOs.Asset;

public record CreateAssetDTO(string Assetname, int? AssettypeId, DateOnly? Purchasedate, DateOnly? Warrantyexpiry, string? Status, int? AssignedClassId);
