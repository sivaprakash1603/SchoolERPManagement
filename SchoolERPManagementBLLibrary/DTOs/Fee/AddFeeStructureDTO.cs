namespace SchoolERPManagementBLLibrary.DTOs.Fee;

public record AddFeeStructureDTO(int ClassId, int AcademicYearId, string FeeName, decimal TotalAmount);
