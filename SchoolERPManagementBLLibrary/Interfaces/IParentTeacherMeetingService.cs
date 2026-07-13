using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SchoolERPManagementBLLibrary.DTOs.ParentTeacherMeeting;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IParentTeacherMeetingService
{
    Task GenerateSlotsAsync(int meetingId, CancellationToken ct);

    Task<List<PtmMeetingResponseDTO>> GetUpcomingMeetingsAsync(CancellationToken ct);

    Task<List<PtmSlotResponseDTO>> GetMeetingSlotsAsync(int meetingId, int? teacherId, int? studentId, CancellationToken ct);

    Task<PtmSlotResponseDTO> BookSlotAsync(int slotId, int parentId, int studentId, CancellationToken ct);

    Task CancelBookingAsync(int slotId, int userId, CancellationToken ct);

    Task<List<PtmSlotResponseDTO>> GetMyBookingsAsync(int parentId, CancellationToken ct);
}
