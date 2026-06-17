using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SchoolERPManagementAPI.Converters;

public class IstDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly TimeZoneInfo IstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var inputString = reader.GetString();
        if (string.IsNullOrEmpty(inputString))
        {
            return default;
        }

        // Parse the incoming string as IST and convert to UTC
        var parsedDate = DateTime.Parse(inputString);
        if (parsedDate.Kind == DateTimeKind.Utc)
        {
            return parsedDate;
        }

        // Treat unspecified as IST
        var unspecifiedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecifiedDate, IstTimeZone);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Treat any value coming from DB as UTC, convert to IST
        var utcDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        var istDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, IstTimeZone);
        
        // Write out ISO 8601 string but without 'Z' since it's local IST
        writer.WriteStringValue(istDate.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}
