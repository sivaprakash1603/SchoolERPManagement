using System;
using System.Text.Json.Serialization;

namespace SchoolERPManagementBLLibrary.DTOs.Fee;

public class CheckoutSessionResultDTO
{
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; } = string.Empty;

    [JsonPropertyName("amountPaid")]
    public decimal AmountPaid { get; set; }

    [JsonPropertyName("studentName")]
    public string StudentName { get; set; } = string.Empty;

    [JsonPropertyName("feeName")]
    public string FeeName { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    public CheckoutSessionResultDTO() { }

    public CheckoutSessionResultDTO(string transactionId, decimal amountPaid, string studentName, string feeName, DateTime date)
    {
        TransactionId = transactionId;
        AmountPaid = amountPaid;
        StudentName = studentName;
        FeeName = feeName;
        Date = date;
    }
}
