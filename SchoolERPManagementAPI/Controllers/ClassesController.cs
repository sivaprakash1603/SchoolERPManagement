using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.Class;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClassesController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassesController(IClassService classService)
        {
            _classService = classService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClasses([FromQuery] int? academicYearId, CancellationToken cancellationToken)
        {
            var result = await _classService.GetAllClassesAsync(academicYearId, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassDTO dto, CancellationToken cancellationToken)
        {
            var result = await _classService.CreateClassAsync(dto, cancellationToken);
            return Ok(result);
        }
    }
}
