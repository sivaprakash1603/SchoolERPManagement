using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolERPManagementBLLibrary.DTOs.Notification;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolERPManagementAPI.Services;

public sealed class HomeworkDueReminderService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HomeworkDueReminderService> _logger;

    public HomeworkDueReminderService(IServiceProvider serviceProvider, ILogger<HomeworkDueReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SendDueRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var homeworkRepository = scope.ServiceProvider.GetRequiredService<IRepository<int, Homework>>();
        var studentRepository = scope.ServiceProvider.GetRequiredService<IRepository<int, Student>>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Fetch homework due tomorrow
        var upcomingHomeworks = await homeworkRepository.Query(true)
            .Include(h => h.Homeworksubmissions)
            .Where(h => h.Duedate == tomorrow)
            .ToListAsync(cancellationToken);

        _logger.LogInformation($"Found {upcomingHomeworks.Count} homework assignments due tomorrow.");

        foreach (var homework in upcomingHomeworks)
        {
            // Find students in the class
            var classStudents = await studentRepository.Query(true)
                .Where(s => s.Studentenrollments.Any(e => e.Classid == homework.Classid))
                .ToListAsync(cancellationToken);

            // Filter out students who already submitted
            var submittedStudentIds = homework.Homeworksubmissions.Select(s => s.Studentid).ToHashSet();
            var pendingStudents = classStudents.Where(s => !submittedStudentIds.Contains(s.Id)).ToList();

            if (pendingStudents.Any())
            {
                var targetUserIds = pendingStudents.Select(s => s.Userid).ToList();

                var notificationDto = new SendNotificationDTO(
                    Title: "Homework Deadline Reminder",
                    Message: $"The assignment '{homework.Title}' is due in 24 hours. Please submit it soon.",
                    CreatedByUserId: null,
                    TargetUserIds: targetUserIds
                );

                await notificationService.SendNotificationAsync(notificationDto, cancellationToken);
                _logger.LogInformation($"Sent homework due reminder for '{homework.Title}' to {targetUserIds.Count} students.");
            }
        }
    }
}
