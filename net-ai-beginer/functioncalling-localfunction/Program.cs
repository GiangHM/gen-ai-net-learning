using Azure;
using Azure.AI.Inference;
using functioncalling_localfunction;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using System.Text;

Console.WriteLine("Hello, I'm starting to learn AI!");

using var factory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Trace));

var sourceName = "functioncalling-localfunction";
TracerProvider tracerProvide = OpenTelemetry.Sdk.CreateTracerProviderBuilder()
    .AddSource(sourceName)
    .AddConsoleExporter()
    .Build();

IChatClient chatClient = new ChatCompletionsClient(endpoint: new Uri("https://models.inference.ai.azure.com")
    , new AzureKeyCredential(""))
    .AsChatClient("gpt-4o-mini")
    .AsBuilder()
    //.UseLogging(factory)
    .UseFunctionInvocation()
    .UseOpenTelemetry(sourceName: sourceName
        , configure: options => options.EnableSensitiveData = true)
    //.UseDistributedCache(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())))
    .UseRateLimiting()
    .Build();

var chatOption = new ChatOptions
{
    Tools = [AIFunctionFactory.Create(LocalFunction.Getweather)]
};


var systemMessage = new ChatMessage(Microsoft.Extensions.AI.ChatRole.System,
    "You are a useful chatbot. You know all football player." +
    " If you don't know an answer, say 'I don't know!'." +
    " Always reply in a funny way. Use emojis if possible.");

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
