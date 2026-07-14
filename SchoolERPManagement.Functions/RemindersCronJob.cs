using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SchoolERPManagement.Functions;

public class RemindersCronJob
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    public RemindersCronJob(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
    {
        _logger = loggerFactory.CreateLogger<RemindersCronJob>();
        _httpClient = httpClientFactory.CreateClient();
    }

    [Function("RemindersCronJob")]
    public async Task Run([TimerTrigger("0 0 8,20 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("RemindersCronJob executing at: {executionTime}", DateTime.Now);

        var apiUrl = Environment.GetEnvironmentVariable("API_URL");
        var apiKey = Environment.GetEnvironmentVariable("API_KEY");

        if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("API_URL or API_KEY environment variables are missing.");
            return;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl.TrimEnd('/')}/api/jobs/trigger-reminders");
        request.Headers.Add("X-API-KEY", apiKey);

        try
        {
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully triggered reminders. Status Code: {StatusCode}", response.StatusCode);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to trigger reminders. Status Code: {StatusCode}, Content: {Content}", response.StatusCode, responseContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while making HTTP request to trigger reminders.");
        }

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}