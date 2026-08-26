using System.Text.Json.Serialization;

namespace OpenRouterResponseModels
{
    public sealed class OpenRouterUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens {get; set;}
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens {get; set;}
        [JsonPropertyName("total_tokens")]
        public int TotalTokens {get; set;}
    }
    public sealed class OpenRouterMessage
    {
        [JsonPropertyName("role")]
        public string Role {get; set;} = "";
        [JsonPropertyName("content")]
        public string Content {get; set;} = "";
    }

    public sealed class OpenRouterChoice
    {
        [JsonPropertyName("message")]
        public OpenRouterMessage Message {get; set;} = new();
    }

    public sealed class OpenRouterResponse
    {
        [JsonPropertyName("id")]
        public string Id {get; set;} = "";
        [JsonPropertyName("model")]
        public string Model {get; set;} = "";

        [JsonPropertyName("choices")]
        public List<OpenRouterChoice> Choices {get; set;} = [];

        [JsonPropertyName("usage")]
        public OpenRouterUsage? Usage {get; set;}
    }
}