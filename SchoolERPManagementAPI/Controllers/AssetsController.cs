using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.Asset;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddAsset([FromBody] CreateAssetDTO dto, CancellationToken cancellationToken)
        {
            var result = await _assetService.AddAssetAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpPost("report")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> ReportAssetIssue([FromBody] AssetIssueDTO dto, CancellationToken cancellationToken)
        {
            var result = await _assetService.ReportAssetIssueAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAssets(CancellationToken cancellationToken)
        {
            var result = await _assetService.GetAssetsAsync(cancellationToken);
            return Ok(result);
        }
    }
}
