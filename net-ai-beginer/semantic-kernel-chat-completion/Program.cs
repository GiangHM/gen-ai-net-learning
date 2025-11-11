// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = Host.CreateApplicationBuilder(args);

// AddOllamaChatCompletion
// There are some overload methods
// We can pass a custom HttpClient to AddOllamaChatCompletion
#pragma warning disable SKEXP0070
builder.Services.AddOllamaChatCompletion(
    modelId: "llama3.2",
    endpoint: new Uri("http://localhost:11434/")
    );

builder.Services.AddTransient((serviceProvider) =>
{
    return new Kernel(serviceProvider);
});
Console.WriteLine("Hello, First lesson with semantic kernel!");

var serviceProvider = builder.Services.BuildServiceProvider();
var kernel = serviceProvider.GetRequiredService<Kernel>();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

ChatHistory history = [];

while (true)
{
    // Get user prompt and add to chat history
    Console.WriteLine("Your prompt:");
    var userPrompt = Console.ReadLine() ?? "Hello, how are you?";
    history.AddUserMessage(userPrompt);

    // Stream the AI response and add to chat history
    Console.WriteLine("AI Response:");
    var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
    chatHistory: history,
    kernel: kernel
);
    await foreach (var item in response)
    {
        Console.Write(item);
        history.AddAssistantMessage(item.ToString());

    }
    Console.WriteLine();
}
