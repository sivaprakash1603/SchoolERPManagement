using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.AcademicCalendar;
using SchoolERPManagementBLLibrary.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolERPManagementAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AcademicCalendarController : ControllerBase
{
    private readonly IAcademicCalendarService _calendarService;

    public AcademicCalendarController(IAcademicCalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCalendarEvent([FromBody] CreateCalendarEventDTO dto, CancellationToken cancellationToken)
    {
        var result = await _calendarService.CreateCalendarEventAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCalendarEvent(int id, CancellationToken cancellationToken)
    {
        await _calendarService.DeleteCalendarEventAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("year/{academicYearId}")]
    public async Task<IActionResult> GetAcademicCalendarSummary(int academicYearId, CancellationToken cancellationToken)
    {
        var result = await _calendarService.GetAcademicCalendarSummaryAsync(academicYearId, cancellationToken);
        return Ok(result);
    }
}
