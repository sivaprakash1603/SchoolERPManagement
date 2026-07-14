using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.Interfaces;
using System.Threading.Tasks;

namespace SchoolERPManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemSetupController : ControllerBase
    {
        private readonly ISystemSetupService _systemSetupService;

        public SystemSetupController(ISystemSetupService systemSetupService)
        {
            _systemSetupService = systemSetupService;
        }

        [HttpGet("status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSetupStatus()
        {
            var isComplete = await _systemSetupService.IsSetupCompleteAsync();
            return Ok(new { isComplete });
        }
    }
}
