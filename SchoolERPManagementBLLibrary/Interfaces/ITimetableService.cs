using SchoolERPManagementBLLibrary.DTOs.Timetable;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface ITimetableService
{
    Task<TimetableResponseDTO> CreateTimetableAsync(CreateTimetableDTO dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<TimetableResponseDTO>> GetClassTimetableAsync(int classId, CancellationToken cancellationToken);
}
