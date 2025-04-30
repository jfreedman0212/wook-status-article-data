using System.Text.Json;
using System.Text.Json.Serialization;

namespace WookiepediaStatusArticleData;

public class TimeOnlyConverter(string? serializationFormat) : JsonConverter<TimeOnly>
{
    private readonly string _serializationFormat = serializationFormat ?? "HH:mm";

    public TimeOnlyConverter() : this(null)
    {
    }

    public override TimeOnly Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return TimeOnly.Parse(value!);
    }

    public override void Write(
        Utf8JsonWriter writer,
        TimeOnly value,
        JsonSerializerOptions options
    ) => writer.WriteStringValue(value.ToString(_serializationFormat));
}