using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace LocalAiApi.Services;

public sealed class AiAccessTokenValidator
{
    private readonly byte[] _key;

    public AiAccessTokenValidator(IConfiguration configuration)
    {
        string key = configuration["AiAccess:SigningKey"] ?? "";
        _key = Encoding.UTF8.GetBytes(key);
    }

    public bool IsValid(string? token)
    {
        if (_key.Length < 32 || string.IsNullOrWhiteSpace(token))
            return false;

        string[] parts = token.Split('.');
        if (parts.Length != 2)
            return false;

        try
        {
            byte[] payloadBytes = Convert.FromBase64String(parts[0]);
            byte[] suppliedSignature = Convert.FromBase64String(parts[1]);
            byte[] expectedSignature = HMACSHA256.HashData(_key, payloadBytes);

            if (!CryptographicOperations.FixedTimeEquals(expectedSignature, suppliedSignature))
                return false;

            string payload = Encoding.UTF8.GetString(payloadBytes);
            string[] values = payload.Split(':');
            if (values.Length != 2 || !Guid.TryParse(values[0], out _))
                return false;

            if (!long.TryParse(values[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long expiresAt))
                return false;

            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() <= expiresAt;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
