using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.AcademicYear;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AcademicYearsController : ControllerBase
    {
        private readonly IAcademicYearService _academicYearService;

        public AcademicYearsController(IAcademicYearService academicYearService)
        {
            _academicYearService = academicYearService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAcademicYear([FromBody] CreateAcademicYearDTO dto, CancellationToken cancellationToken)
        {
            var result = await _academicYearService.CreateAcademicYearAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Student,Parent")]
        public async Task<IActionResult> GetAllAcademicYears(CancellationToken cancellationToken)
        {
            var result = await _academicYearService.GetAllAcademicYearsAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id}/set-current")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetCurrentAcademicYear(int id, CancellationToken cancellationToken)
        {
            await _academicYearService.SetCurrentAcademicYearAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
