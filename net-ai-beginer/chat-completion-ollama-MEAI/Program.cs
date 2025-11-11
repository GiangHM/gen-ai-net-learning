using Microsoft.Extensions.AI;
using System.Text;

Console.WriteLine("Hello, I'm starting to learn AI!");

IChatClient client = new OllamaChatClient(
    new Uri("http://localhost:11434/"), "phi4-mini");


var chatHistory = new List<ChatMessage>();

var chatOption = new ChatOptions
{
    Temperature = 0.5f,
    TopP = 0.9f,
    ResponseFormat = ChatResponseFormat.Text
};

var systemMessage = new ChatMessage(ChatRole.System,
    "You are a useful soccer chatbot. You know all football player." +
    " If you don't know an answer, say 'I don't know!'." +
    " Always reply in a funny way. Use emojis if possible." +
    "Just provide the information in short" +
    "1. Name of the player" +
    "2. When was the player born?" );

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
    chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

    Console.WriteLine("AI Response");
    var sb = new StringBuilder();
    var response = client.GetStreamingResponseAsync(chatHistory, chatOption);
    await foreach (var item in response)
    {
        sb.Append(item.Text);
        Console.Write(item.Text);
    }
    Console.WriteLine();

    chatHistory.Add(new ChatMessage(ChatRole.Assistant, sb.ToString()));
}
