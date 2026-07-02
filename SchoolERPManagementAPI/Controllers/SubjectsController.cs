using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.Subject;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectDTO dto, CancellationToken cancellationToken)
    {
        var result = await _subjectService.CreateSubjectAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetSubjectById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSubjects(CancellationToken cancellationToken)
    {
        var result = await _subjectService.GetAllSubjectsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubjectById(int id, CancellationToken cancellationToken)
    {
        var result = await _subjectService.GetSubjectByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSubject(int id, [FromBody] CreateSubjectDTO dto, CancellationToken cancellationToken)
    {
        var result = await _subjectService.UpdateSubjectAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSubject(int id, CancellationToken cancellationToken)
    {
        await _subjectService.DeleteSubjectAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("class/{classId}")]
    public async Task<IActionResult> GetSubjectsByClass(int classId, CancellationToken cancellationToken)
    {
        var result = await _subjectService.GetSubjectsByClassAsync(classId, cancellationToken);
        return Ok(result);
    }
}
