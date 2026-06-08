using SchoolERPManagementBLLibrary.DTOs.StaffAttendance;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IStaffAttendanceService
{
    Task<StaffAttendanceResponseDTO> MarkAttendanceAsync(StaffAttendanceRequestDTO dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<StaffAttendanceResponseDTO>> GetAttendanceByUserAsync(int userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StaffAttendanceResponseDTO>> GetAllAttendanceByDateAsync(DateOnly date, CancellationToken cancellationToken);
}
