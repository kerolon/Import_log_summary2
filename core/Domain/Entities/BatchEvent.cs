using System;
using System.Text.Json.Serialization;
using ils.core.Common;

namespace ils.core.Domain.Entities
{
    public class BatchEvent
    {
        public string Id { get; set; }
        public EventType Type { get; set; }
        public string TypeString { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonConverter(typeof(DateTimeOffsetJsonConverter))]
        public DateTimeOffset DateTime { get; set; }
        public string DatetimeString { get; set; }
        public string From { get; set; }

    }
}
