using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Helpers;

public sealed class JwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user, Role role)
    {
        var secret = GetSecret();
        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };

        var expiresAt = DateTimeOffset.UtcNow.AddHours(GetLifetimeHours()).ToUnixTimeSeconds();
        var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var payload = new Dictionary<string, object>
        {
            ["sub"] = user.Id,
            ["unique_name"] = user.Username,
            ["email"] = user.Email,
            ["role_id"] = role.Id,
            ["role"] = role.Rolename,
            ["iat"] = issuedAt,
            ["exp"] = expiresAt,
            ["iss"] = GetIssuer(),
            ["aud"] = GetAudience()
        };

        var encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        var encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signingInput = $"{encodedHeader}.{encodedPayload}";
        var signature = ComputeSignature(signingInput, secret);

        return $"{signingInput}.{signature}";
    }

    private string GetSecret() => _configuration["Jwt:Key"] ?? _configuration["Jwt:Secret"] ?? "SchoolERPManagementDevelopmentSecretKey1234567890";

    private string GetIssuer() => _configuration["Jwt:Issuer"] ?? "SchoolERPManagement";

    private string GetAudience() => _configuration["Jwt:Audience"] ?? "SchoolERPManagementUsers";

    private int GetLifetimeHours()
    {
        var configured = _configuration["Jwt:LifetimeHours"];
        return int.TryParse(configured, NumberStyles.Integer, CultureInfo.InvariantCulture, out var hours) && hours > 0
            ? hours
            : 24;
    }

    private static string ComputeSignature(string input, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
