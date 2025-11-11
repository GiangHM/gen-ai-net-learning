using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using OpenAI;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.ClientModel;


var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var githubToken = Environment.GetEnvironmentVariable("GitHubToken");
if (string.IsNullOrEmpty(githubToken))
{
    githubToken = config["GitHubToken"];
}

// Create a kernel with Azure OpenAI chat completion
// modelId = deploymentName
var modelId = "gpt-4o-mini";
//config["DeploymentName"] ?? "Phi-3.5-mini-instruct-generic-cpu";
var endpoint = "https://models.inference.ai.azure.com";
//config["Endpoint"] ?? "";


// Create a kernel and chat completion service
var builder = Kernel.CreateBuilder();

// Add plugins to kernel
//builder.Plugins.AddFromType<FlightBookingPlugin>("FlightBookingPlugin");

var kernelPlugin = await OpenApiKernelPluginFactory.CreateFromOpenApiAsync("RestaurantBookingPlugin"
    , "filepaht\\openapi.json"//TODO: replace with your local path
    , executionParameters: new OpenApiFunctionExecutionParameters()
    {
        ServerUrlOverride = new Uri("https://localhost:7027/")
    });

builder.Plugins.Add(kernelPlugin);
// Add OpenAI chat completion service to kernel
builder.AddOpenAIChatCompletion(modelId
    , new OpenAIClient(
        new ApiKeyCredential(githubToken)
        , new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
    );

var kernel = builder.Build();

OpenAIPromptExecutionSettings settings = new()
{
    //MaxTokens = 1000,
    //Temperature = 0.7,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};
// Start a chat loop
var history = new ChatHistory();
history.AddSystemMessage("You're restaurant assistant, you help people to book a reservation");


while (true)
{
    // Get user prompt and add to chat history
    Console.WriteLine("Your prompt:");
    var userPrompt = Console.ReadLine();
    history.AddUserMessage(userPrompt);

    // Stream the AI response and add to chat history
    var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
    Microsoft.SemanticKernel.ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: settings,
        kernel: kernel
    );
    Console.WriteLine("Restaurant Booking Assistant: " + reply.ToString());
    history.AddAssistantMessage(reply.ToString());

    //await foreach (var item in
    //    chatClient.GetStreamingResponseAsync(chatHistory))
    //{
    //    Console.Write(item.Text);
    //    response += item.Text;
    //}
    //chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
    //Console.WriteLine();
}


