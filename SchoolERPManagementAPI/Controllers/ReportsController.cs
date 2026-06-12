using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("fees")]
        public async Task<IActionResult> GetFeeCollectionReport(
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate, 
            CancellationToken cancellationToken)
        {
            var result = await _reportService.GetFeeCollectionReportAsync(startDate, endDate, cancellationToken);
            return Ok(result);
        }

        [HttpGet("attendance/students")]
        public async Task<IActionResult> GetStudentAttendanceReport(
            [FromQuery] DateOnly? startDate, 
            [FromQuery] DateOnly? endDate, 
            CancellationToken cancellationToken)
        {
            var result = await _reportService.GetStudentAttendanceReportAsync(startDate, endDate, cancellationToken);
            return Ok(result);
        }

        [HttpGet("attendance/staff")]
        public async Task<IActionResult> GetStaffAttendanceReport(
            [FromQuery] DateOnly? startDate, 
            [FromQuery] DateOnly? endDate, 
            CancellationToken cancellationToken)
        {
            var result = await _reportService.GetStaffAttendanceReportAsync(startDate, endDate, cancellationToken);
            return Ok(result);
        }

        [HttpGet("exams/{examId}")]
        public async Task<IActionResult> GetExamPerformanceReport(
            int examId, 
            CancellationToken cancellationToken)
        {
            var result = await _reportService.GetExamPerformanceReportAsync(examId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("assets")]
        public async Task<IActionResult> GetAssetInventoryReport(CancellationToken cancellationToken)
        {
            var result = await _reportService.GetAssetInventoryReportAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("fees/query")]
        public async Task<IActionResult> QueryFeePayments(
            [FromQuery] SchoolERPManagementBLLibrary.DTOs.Report.Query.FeePaymentQueryRequest request, 
            CancellationToken cancellationToken)
        {
            var result = await _reportService.QueryFeePaymentsAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("attendance/students/query")]
        public async Task<IActionResult> QueryStudentAttendance(
            [FromQuery] SchoolERPManagementBLLibrary.DTOs.Report.Query.StudentAttendanceQueryRequest request, 
            CancellationToken cancellationToken)
        {
            var result = await _reportService.QueryStudentAttendanceAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("attendance/staff/query")]
        public async Task<IActionResult> QueryStaffAttendance(
            [FromQuery] SchoolERPManagementBLLibrary.DTOs.Report.Query.StaffAttendanceQueryRequest request, 
            CancellationToken cancellationToken)
        {
            var result = await _reportService.QueryStaffAttendanceAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("exams/results/query")]
        public async Task<IActionResult> QueryExamResults(
            [FromQuery] SchoolERPManagementBLLibrary.DTOs.Report.Query.ExamResultQueryRequest request, 
            CancellationToken cancellationToken)
        {
            var result = await _reportService.QueryExamResultsAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("assets/query")]
        public async Task<IActionResult> QueryAssets(
            [FromQuery] SchoolERPManagementBLLibrary.DTOs.Report.Query.AssetQueryRequest request, 
            CancellationToken cancellationToken)
        {
            var result = await _reportService.QueryAssetsAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}
