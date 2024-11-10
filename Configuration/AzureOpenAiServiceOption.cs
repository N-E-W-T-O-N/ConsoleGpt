namespace console_gpt.Configuration
{
    /// <summary>
    /// Configuration options for interacting with OpenAI.
    /// </summary>
    public class AzureOpenAiServiceOption
    {

        ///  modelId:
        //     Azure OpenAI model ID or deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource


        public string ApiKey { get; set; }

        /// <summary>
        /// Endpoint of the AzureOpenAi Client
        /// </summary>
        public string Endpoint { get; set; }
        /// <summary>
        /// Maximum number of tokens to use when calling OpenAI.
        /// </summary>
        /// 
        public int MaxTokens { get; set; }

        /// <summary>
        /// Randomness controls (0.0 - 1.0).
        /// </summary>
        public float Temperature { get; set; }

        /// <summary>
        /// Diversity (0.0 - 1.0).
        /// </summary>
        public float TopP { get; set; }

        /// <summary>
        /// How much to penalize new tokens based on existing frequency in the text so far (0.0 - 2.0).
        /// </summary>
        public float FrequencyPenalty { get; set; }

        /// <summary>
        /// How much to penalize new tokens based on whether they appear in the text so far (0.0 - 2.0).
        /// </summary>
        public float PresencePenalty { get; set; }

        /// <summary>
        /// Name of the chat model to use (e.g. text-davinci-002).
        /// </summary>
        public string ChatModel { get; set; }

        /// <summary>
        /// Name of the text model to use (e.g. text-davinci-002).
        /// </summary>
        public string TextModel { get; set; }

        /// <summary>
        /// Initial prompt for the conversation.
        /// </summary>
        public string SystemPrompt { get; set; }
    }
}
