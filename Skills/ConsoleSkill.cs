
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace console_gpt.Skills
{
    /// <summary>
    /// A Sematic Kernel skill that provides the ability to read and write from the console
    /// </summary>
    public class ConsoleSkill : ISpeechSkill
    {
        private bool _isGoodbye = false;

        /// <summary>
        /// Gets input from the console
        /// </summary>
        [KernelFunction("Get console input.")]
        [Description("Listen")]
        public Task<string> Listen()
        {
            return Task.Run(() =>
            {
                var line = "";

                while (string.IsNullOrWhiteSpace(line))
                {
                    line = Console.ReadLine();
                }

                if (line.ToLower().StartsWith("goodbye"))
                    _isGoodbye = true;

                return line;
            });
        }

        /// <summary>
        /// Writes output to the console
        /// </summary>
        [Description("Write a response to the console.")]
        [KernelFunction("Respond")]
        public Task<string> Respond(string message)
        {
            return Task.Run(() =>
            {
                WriteAIResponse(message);
                return message;
            });
        }

        /// <summary>
        /// Checks if the user said goodbye
        /// </summary>
        [Description("Did the user say goodbye.")]
        [KernelFunction("IsGoodbye")]
        public Task<string> IsGoodbye()
        {
            return Task.FromResult(_isGoodbye ? "true" : "false");
        }

        /// <summary>
        /// Write a response to the console in green.
        /// </summary>
        private void WriteAIResponse(string response)
        {
            // Write the response in Green, then revert the console color
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(response);
            Console.ForegroundColor = oldColor;
        }
    }
}