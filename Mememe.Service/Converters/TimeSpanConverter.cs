using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mememe.Service.Converters
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        private const string TimeSpanPattern = @"hh\:mm\:ss";

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            TimeSpan.TryParseExact(reader.GetString(), TimeSpanPattern, null, out var deserializedTimeSpan)
                ? deserializedTimeSpan
                : TimeSpan.FromMinutes(1);

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(TimeSpanPattern));
        }
    }
}