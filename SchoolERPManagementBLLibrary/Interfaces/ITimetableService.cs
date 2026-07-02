using SchoolERPManagementBLLibrary.DTOs.Timetable;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface ITimetableService
{
    Task<TimetableResponseDTO> CreateTimetableAsync(CreateTimetableDTO dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<TimetableResponseDTO>> GetClassTimetableAsync(int classId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TimetableResponseDTO>> GetTeacherTimetableAsync(int teacherId, CancellationToken cancellationToken);
    
    Task<IReadOnlyList<TeacherRequirementDTO>> GetTeacherRequirementsAsync(int periodsPerDay, int freePeriodsPerStaff, CancellationToken cancellationToken);
    Task<IReadOnlyList<TimetableResponseDTO>> GenerateTimetableAsync(GenerateTimetableRequestDTO request, CancellationToken cancellationToken);
    Task SaveGeneratedTimetableAsync(IReadOnlyList<TimetableResponseDTO> generatedTimetable, CancellationToken cancellationToken);
    Task<TimetableResponseDTO> UpdateTimetableAsync(int id, UpdateTimetableDTO dto, CancellationToken cancellationToken);
}
