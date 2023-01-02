using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Backend.Fx.AspNetCore.ErrorHandling
{
    [PublicAPI]
    public class SerializableError
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }
        
        [JsonPropertyName("errors")]
        public string[] Errors { get; set; }
    }
}