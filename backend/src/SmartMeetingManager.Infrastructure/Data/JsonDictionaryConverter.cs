using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace SmartMeetingManager.Infrastructure.Data;

public class JsonDictionaryConverter : ValueConverter<Dictionary<string, string>?, string?>
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public JsonDictionaryConverter() : base(
        v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
        v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonOptions))
    {
    }
}
