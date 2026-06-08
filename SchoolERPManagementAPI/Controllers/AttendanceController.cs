using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.Attendance;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceDTO dto, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.MarkAttendanceAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetAttendanceByStudent(int studentId, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.GetAttendanceByStudentAsync(studentId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAttendanceByClass(int classId, [FromQuery] DateTime date, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.GetAttendanceByClassAsync(classId, date, cancellationToken);
            return Ok(result);
        }
    }
}
