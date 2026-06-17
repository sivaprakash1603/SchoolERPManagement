using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SchoolERPManagementAPI.Converters;

public class IstNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private static readonly TimeZoneInfo IstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var inputString = reader.GetString();
        if (string.IsNullOrEmpty(inputString))
        {
            return null;
        }

        var parsedDate = DateTime.Parse(inputString);
        if (parsedDate.Kind == DateTimeKind.Utc)
        {
            return parsedDate;
        }

        var unspecifiedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecifiedDate, IstTimeZone);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        var utcDate = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
        var istDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, IstTimeZone);
        
        writer.WriteStringValue(istDate.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}
