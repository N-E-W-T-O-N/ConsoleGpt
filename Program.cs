using System.Reflection;
using console_gpt;
using console_gpt.Configuration;
using console_gpt.Skills;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

// Create the host builder
var builder = Host.CreateDefaultBuilder(args);

// Load the configuration file and user secrets
//
// These need to be set either directly in the configuration.json file or in the user secrets. Details are in
// the configuration.json file.
#pragma warning disable CS8604
var configurationFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "configuration.json");
#pragma warning restore CS8604
builder.ConfigureAppConfiguration((builder) => builder
    .AddJsonFile(configurationFilePath)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>());

// Configure the services for the host
builder.ConfigureServices((context, services) =>
{
    // Setup configuration options
    var configurationRoot = context.Configuration;
    //services.Configure<AzureCognitiveServicesOptions>(configurationRoot.GetSection("AzureCognitiveServices"));
    //services.Configure<OpenAiServiceOptions>(configurationRoot.GetSection("OpenAI"));
    //services.Configure<AzureOpenAiServiceOption>(configurationRoot.GetSection("AzureOpenAI"));
    services.Configure<ChatModel>(configurationRoot.GetSection(ChatModel.Name));
    // Add Semantic Kernel
    //services.AddSingleton<IKernelBuilder>(serviceProvider => Kernel.CreateBuilder());
    services.AddSingleton<Kernel>(serviceProvider =>
    {
        var chatOption = serviceProvider.GetRequiredService<IOptions<ChatModel>>();
        return Kernel.CreateBuilder().AddAIChatModelCompletion(chatOption).Build();
    });
    //services.AddSingleton<Kernel>();
    // Add Native Skills
    // Use one of these 2 lines for the use input or output.
    // Console Skill is for console interactions, AzCognitiveServicesSpeechSkill is to interact using a mic and speakers
    services.AddSingleton<ISpeechSkill, ConsoleSkill>();
    // services.AddSingleton<ISpeechSkill, AzCognitiveServicesSpeechSkill>();
    services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    services.AddSingleton<ChatSkill>();

    //services.Configure<>(configurationRoot.GetSection(key: nameof()));
    //configurationRoot.GetSection(nameof(decimal)).Get<>();
    // Add the primary hosted service to start the loop.
    services.AddHostedService<ConsoleGPTService>();
});

// Build and run the host. This keeps the app running using the HostedService.
var host = builder.Build();
await host.RunAsync();



public static class ServiceExtension
{


    public static IKernelBuilder AddAIChatModelCompletion(this IKernelBuilder kernel, IOptions<ChatModel> chatOptions)
    {
        //IOptions<OpenAiServiceOptions> openAIOptions, IOptions< AzureOpenAiServiceOption > aZOpenAIOptions;
        if (chatOptions.Value.Type == ModelServiceType.AzureOpenAI)
        {
            var model = chatOptions.Value.AzureOpenAI;

            kernel.AddAzureOpenAIChatCompletion(deploymentName:model.ChatModel,endpoint:model.Endpoint,apiKey:model.ApiKey);
        }
        else if (chatOptions.Value.Type == ModelServiceType.OpenAI)
        {
            var model = chatOptions.Value.OpenAI;

            kernel.AddOpenAIChatCompletion(modelId: model.ChatModel,apiKey: model.Key);
        }

        return kernel;
    }
}