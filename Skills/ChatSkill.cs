using System.ComponentModel;
using console_gpt.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace console_gpt.Skills
{
    /// <summary>
    /// A Semantic Kernel skill that interacts with ChatGPT
    /// </summary>
    public class ChatSkill
    {
        private readonly IChatCompletionService _chatCompletion;
        private readonly ChatHistory _chatHistory;
        private readonly PromptExecutionSettings _chatRequestSettings;
        private readonly Kernel _kernel;
        public ChatSkill(Kernel kernel,
            IOptions<ChatModel> chatOptions)

        {
            _kernel = kernel;
            if (chatOptions.Value.Type == ModelServiceType.OpenAI)
            {
                // Set up the chat request settings
                _chatRequestSettings = new OpenAIPromptExecutionSettings()
                {
                    MaxTokens = chatOptions.Value.OpenAI.MaxTokens,
                    Temperature = chatOptions.Value.OpenAI.Temperature,
                    FrequencyPenalty = chatOptions.Value.OpenAI.FrequencyPenalty,
                    PresencePenalty = chatOptions.Value.OpenAI.PresencePenalty,
                    TopP = chatOptions.Value.OpenAI.TopP
                };

                // Configure the semantic kernel
                //semanticKernel.AddOpenAIChatCompletion(openAIOptions.Value.ChatModel, openAIOptions.Value.Key);
                // Set up the chat completion and history - the history is used to keep track of the conversation
                // and is part of the prompt sent to ChatGPT to allow a continuous conversation
                //_chatCompletion = new OpenAIChatCompletionService(chatOptions.Value.OpenAI.ChatModel, chatOptions.Value.OpenAI.Key);
                _chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();

                _chatHistory = new ChatHistory(chatOptions.Value.OpenAI.SystemPrompt);

                //_chatHistory = (OpenAIChatHistory)_chatCompletion.CreateNewChat(openAIOptions.Value.SystemPrompt);
            }
            else if(chatOptions.Value.Type == ModelServiceType.AzureOpenAI)
            {
                // Set up the chat request settings
                _chatRequestSettings = new AzureOpenAIPromptExecutionSettings()
                {
                    MaxTokens = chatOptions.Value.AzureOpenAI.MaxTokens,
                    Temperature = chatOptions.Value.AzureOpenAI.Temperature,
                    FrequencyPenalty = chatOptions.Value.AzureOpenAI.FrequencyPenalty,
                    PresencePenalty = chatOptions.Value.AzureOpenAI.PresencePenalty,
                    TopP = chatOptions.Value.AzureOpenAI.TopP,
#pragma warning disable SKEXP0001
                    // FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
#pragma warning restore SKEXP0001
                };

                // Configure the semantic kernel
                //semanticKernel.Config.AddChatCompletionService() AddAzureOpenAIChatCompletionService("chat", openAIOptions.Value.ChatModel,openAIOptions.Value.Key);

                //_chatCompletion = new AzureOpenAIChatCompletionService(chatOptions.Value.AzureOpenAI.ChatModel, chatOptions.Value.AzureOpenAI.Endpoint, chatOptions.Value.AzureOpenAI.ApiKey);

                // Set up the chat completion and history - the history is used to keep track of the conversation
                // and is part of the prompt sent to ChatGPT to allow a continuous conversation
                //_chatCompletion = semanticKernel.GetService<IChatCompletion>();
                _chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();

                _chatHistory = new ChatHistory(chatOptions.Value.AzureOpenAI.SystemPrompt ?? "YOU ARE CHAT BOT");
            }
        }

        /// <summary>
        /// Send a prompt to the LLM.
        /// </summary>
        [KernelFunction("Prompt"), Description("Send a prompt to the LLM.")]
        //s[return: Description()]
        public async Task<string> Prompt(string prompt)
        {
            var reply = string.Empty;
            try
            {
                // Add the question as a user message to the chat history, then send everything to OpenAI.
                // The chat history is used as context for the prompt 
                _chatHistory.AddUserMessage(prompt);
                var response = await _chatCompletion.GetChatMessageContentsAsync(_chatHistory, _chatRequestSettings);

                reply = response.Last().Content;
                // Add the interaction to the chat history.
                _chatHistory.AddAssistantMessage(reply!);
            }
            catch (KernelException aiex)
            {
                // Reply with the error message if there is one
                reply = $"OpenAI returned an error ({aiex.Message}). Please try again.";
            }

            return reply;
        }

        /// <summary>
        /// Log the history of the chat with the LLM.
        /// This will log the system prompt that configures the chat, along with the user and assistant messages.
        /// </summary>
        [KernelFunction("Log the history of the chat with the LLM.")]
        [Description("LogChatHistory")]
        public Task LogChatHistory()
        {
            Console.WriteLine();
            Console.WriteLine("Chat history:");
            Console.WriteLine();

            // Log the chat history including system, user and assistant (AI) messages
            foreach (var message in _chatHistory.AsEnumerable())
            {
                // Depending on the role, use a different color
                var role = message.Role;

                string roleDescription;
                ConsoleColor color;

                switch (role.Label)
                {
                    case "system":
                        roleDescription = "System:    ";
                        color = ConsoleColor.Blue;
                        break;

                    case "assistant":
                        roleDescription = "Assistant: ";
                        color = ConsoleColor.Green;
                        break;

                    case "user":
                        roleDescription = "User:     ";
                        color = ConsoleColor.Yellow;
                        break;

                    case "tool":
                        roleDescription = "Tool:     ";
                        color = ConsoleColor.Magenta;
                        break;

                    default:
                        roleDescription = "Unknown:  ";
                        color = ConsoleColor.Gray;
                        break;
                }

                // Apply the console color and print the role description
                Console.ForegroundColor = color;

                // Write the role and the message
                Console.WriteLine($"{roleDescription}{message.Content}");
                // Reset the console color to its default
                Console.ResetColor();
            }

            return Task.CompletedTask;
        }
    }
}