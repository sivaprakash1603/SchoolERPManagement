using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAdminDashboard([FromQuery] int? academicYearId, CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetAdminDashboardMetricsAsync(academicYearId, cancellationToken);
            return Ok(result);
        }
    }
}
