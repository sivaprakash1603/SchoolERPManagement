namespace SchoolERPManagementBLLibrary.DTOs.Fee;

public record FeeStructureResponseDTO(int Id, int ClassId, int AcademicYearId, string FeeName, decimal TotalAmount, DateTime? DueDate);
