using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.StaffAttendance;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class StaffAttendanceController : ControllerBase
{
    private readonly IStaffAttendanceService _staffAttendanceService;

    public StaffAttendanceController(IStaffAttendanceService staffAttendanceService)
    {
        _staffAttendanceService = staffAttendanceService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkAttendance([FromBody] StaffAttendanceRequestDTO dto, CancellationToken cancellationToken)
    {
        var result = await _staffAttendanceService.MarkAttendanceAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetAttendanceByUser(int userId, CancellationToken cancellationToken)
    {
        var result = await _staffAttendanceService.GetAttendanceByUserAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("date/{date}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAttendanceByDate(DateOnly date, CancellationToken cancellationToken)
    {
        var result = await _staffAttendanceService.GetAllAttendanceByDateAsync(date, cancellationToken);
        return Ok(result);
    }
}
