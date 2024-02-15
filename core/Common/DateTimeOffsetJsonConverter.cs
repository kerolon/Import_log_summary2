using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ils.core.Common
{
    public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.GetString()! == null) return DateTime.MinValue;
            return DateTimeOffset.ParseExact(reader.GetString()!,
                "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer,DateTimeOffset dateTimeValue,JsonSerializerOptions options)
        {
            writer.WriteStringValue(dateTimeValue.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture));
            return;
        }
    }
}
