using System;
using System.Text.Json;

public record CheckoutSessionResultDTO(
    string TransactionId,
    decimal AmountPaid,
    string StudentName,
    string FeeName,
    DateTime Date
);

var dto = new CheckoutSessionResultDTO("tx_123", 100.5m, "John Doe", "Term 1 Fee", DateTime.UtcNow);
var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
Console.WriteLine(JsonSerializer.Serialize(dto, options));
