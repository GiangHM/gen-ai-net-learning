using Azure;
using Azure.AI.Inference;
using functioncallingapi;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using System.Text;

Console.WriteLine("Hello, I'm starting to learn AI!");

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var githubToken = Environment.GetEnvironmentVariable("GitHubToken");
if (string.IsNullOrEmpty(githubToken))
{
    githubToken = config["GitHubToken"];
}
// Setup service collection for dependency injection
var serviceCollection = new ServiceCollection();
serviceCollection.AddHttpClient("WeatherApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7074/");
});

// Build the service provider
var serviceProvider = serviceCollection.BuildServiceProvider();

// Get the HTTP client factory
var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

// Create an instance of LocalFunction with the HTTP client factory
var localFunction = new LocalFunction(httpClientFactory);

using var factory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Trace));

var sourceName = "functioncalling-api";
TracerProvider tracerProvide = OpenTelemetry.Sdk.CreateTracerProviderBuilder()
    .AddSource(sourceName)
    .AddConsoleExporter()
    .Build();

IChatClient chatClient = new ChatCompletionsClient(endpoint: new Uri("https://models.inference.ai.azure.com")
    , new AzureKeyCredential(githubToken))
    .AsChatClient("gpt-4o-mini")
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();

// Define a synchronous wrapper for the async method
Func<string, string> getWeatherSync = (city) => 
{
    return LocalFunction.Getweather(city).GetAwaiter().GetResult();
};

var chatOption = new ChatOptions
{
    Tools = [AIFunctionFactory.Create(getWeatherSync)]
};


var systemMessage = new ChatMessage(Microsoft.Extensions.AI.ChatRole.System,
    "You are a useful weather chatbot. Depend on the weather, you suggest some outdoor activity." +
    " If you don't know an answer, say 'I don't know!'.");

var chatHistory = new List<ChatMessage>();
chatHistory.Add(systemMessage);

while (true)
{
    Console.Write("Q: ");
    var userPrompt = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userPrompt))
    {
        Console.WriteLine("Please enter a valid prompt.");
        continue;
    }
    chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, userPrompt));

    Console.WriteLine("AI Response");
    var sb = new StringBuilder();
    var response = chatClient.GetStreamingResponseAsync(chatHistory, chatOption);
    await foreach (var item in response)
    {
        sb.Append(item.Text);
        Console.Write(item.Text);
    }
    Console.WriteLine();

    chatHistory.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.Assistant, sb.ToString()));
}
