using System.Text.Json.Serialization;

namespace WebPaymentTest.Models
{
    public class YooKassaHook
    {
        [JsonPropertyName("event")]
        public string Event { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }
    }

    public class YooKassaHookObject
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
