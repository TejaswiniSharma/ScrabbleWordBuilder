using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScrabbleWordBuilder.Models;

public class LetterCharConverter : JsonConverter<char>
{
    public override char Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        if (string.IsNullOrEmpty(value) || value.Length != 1)
            throw new JsonException($"Expected single character, got: {value}");
        return char.ToUpper(value[0]);
    }

    public override void Write(Utf8JsonWriter writer, char value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
