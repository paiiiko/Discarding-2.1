using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discarding_2._1
{
    [JsonObject]
    public class Message
    {
        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
