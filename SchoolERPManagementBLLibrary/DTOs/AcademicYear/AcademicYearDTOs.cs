namespace SchoolERPManagementBLLibrary.DTOs.AcademicYear;

public record CreateAcademicYearDTO(string YearName, DateOnly StartDate, DateOnly EndDate);
public record AcademicYearResponseDTO(int Id, string YearName, DateOnly StartDate, DateOnly EndDate, bool IsCurrent);
