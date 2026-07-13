using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.ParentTeacherMeeting;
using SchoolERPManagementBLLibrary.Interfaces;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolERPManagementAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ParentTeacherMeetingController : ControllerBase
{
    private readonly IParentTeacherMeetingService _ptmService;
    private readonly IParentService _parentService;

    public ParentTeacherMeetingController(IParentTeacherMeetingService ptmService, IParentService parentService)
    {
        _ptmService = ptmService;
        _parentService = parentService;
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming(CancellationToken cancellationToken)
    {
        var result = await _ptmService.GetUpcomingMeetingsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{meetingId}/slots")]
    public async Task<IActionResult> GetSlots(int meetingId, [FromQuery] int? teacherId, [FromQuery] int? studentId, CancellationToken cancellationToken)
    {
        var result = await _ptmService.GetMeetingSlotsAsync(meetingId, teacherId, studentId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("book")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> BookSlot([FromBody] BookSlotRequestDTO dto, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var parentId = await _parentService.GetParentIdByUserIdAsync(userId, cancellationToken);
        if (parentId == null)
            return Unauthorized(new { message = "Only parents can book slots." });
        var result = await _ptmService.BookSlotAsync(dto.SlotId, parentId.Value, dto.StudentId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("cancel/{slotId}")]
    [Authorize(Roles = "Admin,Parent")]
    public async Task<IActionResult> CancelSlot(int slotId, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _ptmService.CancelBookingAsync(slotId, userId, cancellationToken);
        return NoContent();
    }

    [HttpGet("my-bookings")]
    public async Task<IActionResult> GetMyBookings(CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var parentId = await _parentService.GetParentIdByUserIdAsync(userId, cancellationToken);
        if (parentId == null)
            return Unauthorized(new { message = "Only parents can view bookings." });
        var result = await _ptmService.GetMyBookingsAsync(parentId.Value, cancellationToken);
        return Ok(result);
    }

}
