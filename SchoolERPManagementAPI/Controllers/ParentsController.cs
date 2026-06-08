using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.Parent;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ParentsController : ControllerBase
    {
        private readonly IParentService _parentService;

        public ParentsController(IParentService parentService)
        {
            _parentService = parentService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetParentById(int id, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

            if (userRole == "Parent")
            {
                var parentId = await _parentService.GetParentIdByUserIdAsync(userId, cancellationToken);
                if (id != parentId)
                {
                    return Forbid();
                }
            }

            var result = await _parentService.GetParentByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}/children")]
        public async Task<IActionResult> GetChildren(int id, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

            if (userRole == "Parent")
            {
                var parentId = await _parentService.GetParentIdByUserIdAsync(userId, cancellationToken);
                if (id != parentId)
                {
                    return Forbid();
                }
            }

            var result = await _parentService.GetChildrenAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddParent([FromBody] CreateParentDTO dto, CancellationToken cancellationToken)
        {
            var result = await _parentService.AddParentAsync(dto, cancellationToken);
            return Ok(result);
        }
    }
}
