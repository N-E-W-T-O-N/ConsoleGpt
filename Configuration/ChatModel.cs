using System.Text.Json.Serialization;

namespace console_gpt.Configuration
{
    public class ChatModel
    {
        public const string Name = "ChatModel";
        public string Type { get; set; } = ModelServiceType.AzureOpenAI;
        [JsonPropertyName("AzureOpenAI")]
        public AzureOpenAiServiceOption AzureOpenAI { get; set; }
        public OpenAiServiceOptions OpenAI { get; set; }
    }

    public class ModelServiceType
    {
        public const string AzureOpenAI = "AzureOpenAI";
        public const string OpenAI = "OpenAI";
    }
}
