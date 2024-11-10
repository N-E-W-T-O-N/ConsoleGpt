using System.Reflection;

using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using console_gpt.Skills;
namespace console_gpt
{
    /// <summary>
    /// This is the main application service.
    /// This takes console input, then sends it to the configured AI service, and then prints the response.
    /// All conversation history is maintained in the chat history.
    /// </summary>
    internal class ConsoleGPTService : IHostedService
    {
        private readonly ISpeechSkill _speechSkill;
        private readonly ChatSkill _chatSkill;
        private readonly IHostApplicationLifetime _lifeTime;

        private readonly Kernel _kernel;
        // Uncomment this to create a function that converts text to a poem
        // private readonly ISKFunction _poemFunction;

        public ConsoleGPTService(Kernel semanticKernel,
                                 ISpeechSkill speechSkill,
                                 ChatSkill chatSkill,
                                 //  IOptions<OpenAiServiceOptions> openAIOptions,
                                 IHostApplicationLifetime lifeTime)
        {
            _kernel = semanticKernel;
            _lifeTime = lifeTime;
            _speechSkill = speechSkill;
            _chatSkill = chatSkill;

            //_kernel.Plugins.AddFromType<ISpeechSkill>();
            // Import the skills to load the semantic kernel functions
            //var speech = this._kernel.Plugins.AddFromType<ISpeechSkill>();
            //var speech = _semanticKernel.Plugins.AddFromType<ISpeechSkill>();
            //var ChatSkill = this._kernel.Plugins.AddFromType<ChatSkill>();
            //_kernel.Plugins.AddFromType<ChatSkill>();
            //Kernel kernel = _semanticKernel.Build();
            // Uncomment this to create a function that converts text to a poem
            // _semanticKernel.Config.AddOpenAITextCompletionService("text", openAIOptions.Value.TextModel, 
            //     openAIOptions.Value.Key);

            // var poemPrompt = """
            // Take this "{{$INPUT}}" and convert it to a poem in iambic pentameter.
            // """;

            // _poemFunction = _semanticKernel.CreateSemanticFunction(poemPrompt, maxTokens: openAIOptions.Value.MaxTokens,
            //     temperature: openAIOptions.Value.Temperature, frequencyPenalty: openAIOptions.Value.FrequencyPenalty,
            //     presencePenalty: openAIOptions.Value.PresencePenalty, topP: openAIOptions.Value.TopP);
        }

        /// <summary>
        /// Start the service.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => ExecuteAsync(cancellationToken), cancellationToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop a running service.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// The main execution loop. This awaits input and responds to it using semantic kernel functions.
        /// </summary>
        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Write to the console that the conversation is beginning

            //await _kernel.InvokeAsync(pluginName: "SpeechSkill", functionName: "Respond", new KernelArguments()
            //{
            //    {"Input","Hello. Ask me a question or say goodbye to exit."}
            //});
            //(, _speechSkill["Respond"]);

            // Loop till we are cancelled
            while (!cancellationToken.IsCancellationRequested)
            {
                // Create our pipeline
                //KernelFunction[] pipeline = { _speechSkill["Listen"], _chatSkill["Prompt"], _speechSkill["Respond"] };

                // Uncomment the following line to include the poem function in the pipeline
                // pipeline = pipeline.Append(_poemFunction).Append(_speechSkill["Respond"]).ToArray();
                // Run the pipeline
                var result = await _kernel.InvokePromptAsync("HI HOW ARE YOU",cancellationToken:cancellationToken);
                Console.WriteLine(result.GetValue<string>());
                var Listen = KernelFunctionFactory.CreateFromMethod(method: _speechSkill.Listen, "Listen", "Listen To User Input",returnParameter:new(metadata:new KernelReturnParameterMetadata()){});
                var Prompt = KernelFunctionFactory.CreateFromMethod(method: _chatSkill.Prompt, "Prompt", "Send a User Prompt prompt to the LLM.",new List<KernelParameterMetadata>()
                {
                    new("prompt"){Description = "Prompt Send BY USER"}
                });
                var Respond = KernelFunctionFactory.CreateFromMethod(method: _speechSkill.Respond, "Respond", "Write Back To user",new List<KernelParameterMetadata>(){new("message"){ParameterType =typeof(String)}});
                var IsGoodBye = KernelFunctionFactory.CreateFromMethod(method: _speechSkill.IsGoodbye, "IsGoodBye", "Is the user say goodbye.");
                var LogChatHistory =
                    KernelFunctionFactory.CreateFromMethod(method: _chatSkill.LogChatHistory, "LogChatHistory", "Log the history of the chat with the LLM.");
                /*KernelFunctionFactory.CreateFromMethod(async () =>
                {
                    await Task.Delay(1);
                    Console.WriteLine("happy");
                },"Happy");*/
                KernelFunction pipeLine = KernelFunctionCombinators.Pipe([Listen, Prompt, Respond]);
                //_kernel.InvokeAsync()_kernel.in _kernel.InvokePromptAsync()
                var t = await _kernel.InvokeAsync(pipeLine, new KernelArguments(), cancellationToken);

                // Did we say goodbye? If so, exit
                var goodbyeContext = await _kernel.InvokeAsync(IsGoodBye, new KernelArguments(), cancellationToken);
                var isGoodbye = bool.Parse(goodbyeContext.GetValue<string>());

                // If the user says goodbye, end the chat
                if (isGoodbye)
                {
                    // Log the history so we can see the prompts used
                    await _kernel.InvokeAsync(LogChatHistory, new KernelArguments(), cancellationToken);

                    // Stop the application
                    _lifeTime.StopApplication();
                    break;
                }
            }
        }

        public static class KernelFunctionCombinators
        {
            /// <summary>
            /// Invokes a pipeline of functions, running each in order and passing the output from one as the first argument to the next.
            /// </summary>
            /// <param name="functions">The pipeline of functions to invoke.</param>
            /// <param name="kernel">The kernel to use for the operations.</param>
            /// <param name="arguments">The arguments.</param>
            /// <param name="cancellationToken">The cancellation token to monitor for a cancellation request.</param>
            public static Task<FunctionResult> InvokePipelineAsync(
                IEnumerable<KernelFunction> functions, Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken) =>
                Pipe(functions).InvokeAsync(kernel, arguments, cancellationToken);

            /// <summary>
            /// Invokes a pipeline of functions, running each in order and passing the output from one as the named argument to the next.
            /// </summary>
            /// <param name="functions">The sequence of functions to invoke, along with the name of the argument to assign to the result of the function's invocation.</param>
            /// <param name="kernel">The kernel to use for the operations.</param>
            /// <param name="arguments">The arguments.</param>
            /// <param name="cancellationToken">The cancellation token to monitor for a cancellation request.</param>
            public static Task<FunctionResult> InvokePipelineAsync(
                IEnumerable<(KernelFunction Function, string OutputVariable)> functions, Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken) =>
                Pipe(functions).InvokeAsync(kernel, arguments, cancellationToken);

            /// <summary>
            /// Creates a function whose invocation will invoke each of the supplied functions in sequence.
            /// </summary>
            /// <param name="functions">The pipeline of functions to invoke.</param>
            /// <param name="functionName">The name of the combined operation.</param>
            /// <param name="description">The description of the combined operation.</param>
            /// <returns>The result of the final function.</returns>
            /// <remarks>
            /// The result from one function will be fed into the first argument of the next function.
            /// </remarks>
            public static KernelFunction Pipe(
                IEnumerable<KernelFunction> functions,
                string? functionName = null,
                string? description = null)
            {
                ArgumentNullException.ThrowIfNull(functions);

                KernelFunction[] funcs = functions.ToArray();
                Array.ForEach(funcs, f => ArgumentNullException.ThrowIfNull(f));

                var funcsAndVars = new (KernelFunction Function, string OutputVariable)[funcs.Length];
                for (int i = 0; i < funcs.Length; i++)
                {
                    string p = "";
                    if (i < funcs.Length - 1)
                    {
                        var parameters = funcs[i + 1].Metadata.Parameters;
                        if (parameters.Count > 0)
                        {
                            p = parameters[0].Name;
                        }
                    }

                    funcsAndVars[i] = (funcs[i], p);
                }

                return Pipe(funcsAndVars, functionName, description);
            }

            /// <summary>
            /// Creates a function whose invocation will invoke each of the supplied functions in sequence.
            /// </summary>
            /// <param name="functions">The pipeline of functions to invoke, along with the name of the argument to assign to the result of the function's invocation.</param>
            /// <param name="functionName">The name of the combined operation.</param>
            /// <param name="description">The description of the combined operation.</param>
            /// <returns>The result of the final function.</returns>
            /// <remarks>
            /// The result from one function will be fed into the first argument of the next function.
            /// </remarks>
            public static KernelFunction Pipe(
                IEnumerable<(KernelFunction Function, string OutputVariable)> functions,
                string? functionName = null,
                string? description = null)
            {
                ArgumentNullException.ThrowIfNull(functions);

                (KernelFunction Function, string OutputVariable)[] arr = functions.ToArray();
                Array.ForEach(arr, f =>
                {
                    ArgumentNullException.ThrowIfNull(f.Function);
                    ArgumentNullException.ThrowIfNull(f.OutputVariable);
                });

                return KernelFunctionFactory.CreateFromMethod(async (Kernel kernel, KernelArguments arguments) =>
                {
                    FunctionResult? result = null;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        result = await arr[i].Function.InvokeAsync(kernel, arguments).ConfigureAwait(false);
                        if (i < arr.Length - 1)
                        {
                            arguments[arr[i].OutputVariable] = result.GetValue<object>();
                        }
                    }

                    return result;
                }, functionName, description);
            }
        }

    }
}
