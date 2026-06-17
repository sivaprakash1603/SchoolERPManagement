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
        private readonly IPdfReportService _pdfReportService;

        public ReportsController(IReportService reportService, IPdfReportService pdfReportService)
        {
            _reportService = reportService;
            _pdfReportService = pdfReportService;
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

        // --- PDF EXPORT ENDPOINTS --- //

        [HttpGet("fees/export/pdf")]
        public async Task<IActionResult> ExportFeeCollectionPdf(
            [FromQuery] SchoolERPManagementBLLibrary.DTOs.Report.Query.FeePaymentQueryRequest request,
            CancellationToken cancellationToken)
        {
            request.PageNumber = 1;
            request.PageSize = int.MaxValue;
            var result = await _reportService.QueryFeePaymentsAsync(request, cancellationToken);
            var pdfBytes = _pdfReportService.GenerateFeeCollectionPdf(result.Items.ToList());
            return File(pdfBytes, "application/pdf", "fee-collection-report.pdf");
        }

        [HttpGet("attendance/students/export/pdf")]
        public async Task<IActionResult> ExportStudentAttendancePdf(
            [FromQuery] SchoolERPManagementBLLibrary.DTOs.Report.Query.StudentAttendanceQueryRequest request,
            CancellationToken cancellationToken)
        {
            request.PageNumber = 1;
            request.PageSize = int.MaxValue;
            var result = await _reportService.QueryStudentAttendanceAsync(request, cancellationToken);
            var pdfBytes = _pdfReportService.GenerateStudentAttendancePdf(result.Items.ToList());
            return File(pdfBytes, "application/pdf", "student-attendance-report.pdf");
        }

        [HttpGet("attendance/staff/export/pdf")]
        public async Task<IActionResult> ExportStaffAttendancePdf(
            [FromQuery] SchoolERPManagementBLLibrary.DTOs.Report.Query.StaffAttendanceQueryRequest request,
            CancellationToken cancellationToken)
        {
            request.PageNumber = 1;
            request.PageSize = int.MaxValue;
            var result = await _reportService.QueryStaffAttendanceAsync(request, cancellationToken);
            var pdfBytes = _pdfReportService.GenerateStaffAttendancePdf(result.Items.ToList());
            return File(pdfBytes, "application/pdf", "staff-attendance-report.pdf");
        }

        [HttpGet("exams/results/export/pdf")]
        public async Task<IActionResult> ExportExamResultsPdf(
            [FromQuery] SchoolERPManagementBLLibrary.DTOs.Report.Query.ExamResultQueryRequest request,
            CancellationToken cancellationToken)
        {
            request.PageNumber = 1;
            request.PageSize = int.MaxValue;
            var result = await _reportService.QueryExamResultsAsync(request, cancellationToken);
            var pdfBytes = _pdfReportService.GenerateExamResultsPdf(result.Items.ToList());
            return File(pdfBytes, "application/pdf", "exam-results-report.pdf");
        }

        [HttpGet("assets/export/pdf")]
        public async Task<IActionResult> ExportAssetsPdf(
            [FromQuery] SchoolERPManagementBLLibrary.DTOs.Report.Query.AssetQueryRequest request,
            CancellationToken cancellationToken)
        {
            request.PageNumber = 1;
            request.PageSize = int.MaxValue;
            var result = await _reportService.QueryAssetsAsync(request, cancellationToken);
            var pdfBytes = _pdfReportService.GenerateAssetsPdf(result.Items.ToList());
            return File(pdfBytes, "application/pdf", "assets-report.pdf");
        }
    }
}
