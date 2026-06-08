using SchoolERPManagementBLLibrary.DTOs.AcademicYear;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IAcademicYearService
{
    Task<AcademicYearResponseDTO> CreateAcademicYearAsync(CreateAcademicYearDTO dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<AcademicYearResponseDTO>> GetAllAcademicYearsAsync(CancellationToken cancellationToken);
    Task SetCurrentAcademicYearAsync(int id, CancellationToken cancellationToken);
}
