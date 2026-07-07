using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolERPManagementBLLibrary.DTOs.Notification;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolERPManagementAPI.Services;

public sealed class FeesDueReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FeesDueReminderService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    public FeesDueReminderService(IServiceProvider serviceProvider, ILogger<FeesDueReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Fees Due Reminder Background Service starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendDueRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while executing fees due reminders.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("Fees Due Reminder Background Service stopping.");
    }

    public async Task SendDueRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var feeStructureRepository = scope.ServiceProvider.GetRequiredService<IRepository<int, Feestructure>>();
        var feePaymentRepository = scope.ServiceProvider.GetRequiredService<IRepository<int, Feepayment>>();
        var studentRepository = scope.ServiceProvider.GetRequiredService<IRepository<int, Student>>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.UtcNow;
        var thresholdStart = now.AddDays(3).AddHours(-1);
        var thresholdEnd = now.AddDays(3);

        // Fetch fee structures due in exactly 3 days
        var upcomingFees = await feeStructureRepository.Query(true)
            .Where(f => f.Duedate != null && f.Duedate >= thresholdStart && f.Duedate <= thresholdEnd)
            .ToListAsync(cancellationToken);

        _logger.LogInformation($"Found {upcomingFees.Count} fee structures due in the next 3 days.");

        foreach (var fee in upcomingFees)
        {
            // Find students in the class with their parents
            var classStudents = await studentRepository.Query(true)
                .Include(s => s.Studentparents)
                .ThenInclude(sp => sp.Parent)
                .Where(s => s.Studentenrollments.Any(e => e.Classid == fee.Classid))
                .ToListAsync(cancellationToken);

            foreach (var student in classStudents)
            {
                // Calculate total payments made by this student for this fee structure
                var studentPayments = await feePaymentRepository.Query(true)
                    .Where(p => p.Studentid == student.Id && p.Feestructureid == fee.Id)
                    .ToListAsync(cancellationToken);

                var totalPaid = studentPayments.Sum(p => p.Amountpaid);

                if (totalPaid < fee.Totalamount)
                {
                    var remaining = fee.Totalamount - totalPaid;
                    var targetUserIds = new List<int> { student.Userid };
                    var parentUserIds = student.Studentparents
                        .Where(sp => sp.Parent != null)
                        .Select(sp => sp.Parent.Userid)
                        .ToList();
                    targetUserIds.AddRange(parentUserIds);

                    var notificationDto = new SendNotificationDTO(
                        Title: "Fee Payment Due Reminder",
                        Message: $"Payment of ₹{remaining} is outstanding for fee '{fee.Feename}'. The due date is {fee.Duedate:yyyy-MM-dd}.",
                        CreatedByUserId: null,
                        TargetUserIds: targetUserIds
                    );

                    await notificationService.SendNotificationAsync(notificationDto, cancellationToken);
                }
            }
        }
    }
}
