using System.Text.Json.Serialization;

namespace SlotManagement.Dtos
{
    public class SlotDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("StartTime")]
        public string Start { get; set; } = "";

        [JsonPropertyName("EndTime")]
        public string End { get; set; } = "";

        [JsonPropertyName("available")]
        public bool Available { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("created_by")]
        public int? CreatedBy { get; set; }

        [JsonPropertyName("created_by_name")]
        public string? CreatedByName { get; set; }

        [JsonPropertyName("created_on")]
        public string CreatedOn { get; set; } = "";

        [JsonPropertyName("updated_on")]
        public string ModifiedOn { get; set; } = "";
    }
}
