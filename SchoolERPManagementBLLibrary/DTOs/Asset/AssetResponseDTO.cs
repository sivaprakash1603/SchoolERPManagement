namespace SchoolERPManagementBLLibrary.DTOs.Asset;

public record AssetResponseDTO(int Id, string Assetname, int? AssettypeId, DateOnly? Purchasedate, DateOnly? Warrantyexpiry, string? Status, int? AssignedClassId);
