using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementAPI.Attributes;
using SchoolERPManagementAPI.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolERPManagementAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiKeyAuthorize]
public class JobsController : ControllerBase
{
    private readonly HomeworkDueReminderService _homeworkService;
    private readonly FeesDueReminderService _feesService;

    public JobsController(HomeworkDueReminderService homeworkService, FeesDueReminderService feesService)
    {
        _homeworkService = homeworkService;
        _feesService = feesService;
    }

    [HttpPost("trigger-reminders")]
    public async Task<IActionResult> TriggerReminders(CancellationToken cancellationToken)
    {
        try
        {
            await _homeworkService.SendDueRemindersAsync(cancellationToken);
            await _feesService.SendDueRemindersAsync(cancellationToken);
            
            return Ok(new { message = "Reminders triggered successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while triggering reminders.", details = ex.Message });
        }
    }
}
