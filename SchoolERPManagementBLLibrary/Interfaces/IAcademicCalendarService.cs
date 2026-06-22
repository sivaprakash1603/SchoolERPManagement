using System.Threading;
using System.Threading.Tasks;
using SchoolERPManagementBLLibrary.DTOs.AcademicCalendar;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IAcademicCalendarService
{
    Task<CalendarEventResponseDTO> CreateCalendarEventAsync(CreateCalendarEventDTO dto, CancellationToken cancellationToken);
    Task DeleteCalendarEventAsync(int id, CancellationToken cancellationToken);
    Task<AcademicCalendarSummaryDTO> GetAcademicCalendarSummaryAsync(int academicYearId, CancellationToken cancellationToken);
}
