// See https://aka.ms/new-console-template for more information

using ChatCompletionAgentSample;
using ChatCompletionAgentSample.Plugins;
using Microsoft.Agents.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;
using System.ClientModel;
using System.Net;

ChatCompletionModelSettings settings = new ChatCompletionModelSettings();

Console.WriteLine("Initialize plugins...");
GitHubSettings gitHubSettings = settings.GetSettings<GitHubSettings>();
GitHubPlugin githubPlugin = new GitHubPlugin(gitHubSettings);

Console.WriteLine("Creating kernel...");
IKernelBuilder builder = Kernel.CreateBuilder();

builder.Plugins.AddFromObject(githubPlugin);

// Add OpenAI chat completion service to kernel
builder.AddOpenAIChatCompletion(settings.GitHubModelAI.ChatModel
    , new OpenAIClient(
        new ApiKeyCredential(settings.GitHubModelAI.Token)
        , new OpenAIClientOptions { Endpoint = new Uri(settings.GitHubModelAI.Endpoint) })
    );

var kernel = builder.Build();

Console.WriteLine("Defining agent...");

ChatCompletionAgent chatAgent = new()
{
    Name = "SampleAgent",
    Instructions = "You are an agent designed to query and retrieve information from a single GitHub repository in a read-only manner." +
    " You are also able to access the profile of the active user." +
    " Use the current date and time to provide up-to-date details or time-sensitive responses." +
    "The repository you are querying is a public repository with the following name: {{$repository}}." +
    "The current date and time is: {{$now}}.",
    Kernel = kernel,
    Arguments = new KernelArguments(new OpenAIPromptExecutionSettings(){ FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
    {
        { "repository", "microsoft/semantic-kernel" }
    }
};

Console.WriteLine("Agent ready!");

ChatHistoryAgentThread chatHistoryAgentThread = new();

bool isComplete = false;

do
{
    Console.WriteLine();
    Console.Write("> ");
    string input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }
    if (input.Trim().Equals("EXIT", StringComparison.OrdinalIgnoreCase))
    {
        isComplete = true;
        break;
    }

    var message = new ChatMessageContent(AuthorRole.User, input);

    Console.WriteLine();

    DateTime now = DateTime.Now;
    KernelArguments arguments = new()
    {
                { "now", $"{now.ToShortDateString()} {now.ToShortTimeString()}" }
    };
    await foreach (ChatMessageContent response in chatAgent.InvokeAsync(message, chatHistoryAgentThread, options: new() { KernelArguments = arguments }))
    {
        // Display response.
        Console.WriteLine($"{response.Content}");
    }

} while (!isComplete);


