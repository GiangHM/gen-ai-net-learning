using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using practice_sk_plugins_foundrylocal;
using System.ClientModel;


string filePath = Path.GetFullPath("appsettings.json");
var config = new ConfigurationBuilder()
    .AddJsonFile(filePath)
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

#region OpenTelemetry
//var resourceBuilder = ResourceBuilder
//    .CreateDefault()
//    .AddService("TelemetryConsoleQuickstart");

//AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

//using var traceProvider = Sdk.CreateTracerProviderBuilder()
//    .SetResourceBuilder(resourceBuilder)
//    .AddSource("Microsoft.SemanticKernel*")
//    .AddConsoleExporter()
//    .Build();

//using var meterProvider = Sdk.CreateMeterProviderBuilder()
//    .SetResourceBuilder(resourceBuilder)
//    .AddMeter("Microsoft.SemanticKernel*")
//    .AddConsoleExporter()
//    .Build();

//using var loggerFactory = LoggerFactory.Create(builder =>
//{
//    // Add OpenTelemetry as a logging provider
//    builder.AddOpenTelemetry(options =>
//    {
//        options.SetResourceBuilder(resourceBuilder);
//        options.AddConsoleExporter();
//        // Format log messages. This is default to false.
//        options.IncludeFormattedMessage = true;
//        options.IncludeScopes = true;
//    });
//    builder.SetMinimumLevel(LogLevel.Information);
//});

#endregion

// Create a kernel and chat completion service
var builder = Kernel.CreateBuilder();

// Add logging to kernel
//builder.Services.AddSingleton(loggerFactory);

// Add plugins to kernel
builder.Plugins.AddFromType<FlightBookingPlugin>("FlightBookingPlugin");
// Add OpenAI chat completion service to kernel
builder.AddOpenAIChatCompletion(modelId
    , new OpenAIClient(
        new ApiKeyCredential(githubToken)
        , new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
    );

var kernel = builder.Build();
kernel.FunctionInvocationFilters.Add(new PermissionFilter());

// Add plugins to kernel
KernelFunction searchFlights = kernel.Plugins.GetFunction("FlightBookingPlugin", "search_flights");
KernelFunction bookFlight = kernel.Plugins.GetFunction("FlightBookingPlugin", "book_flight");

OpenAIPromptExecutionSettings settings = new()
{
    //MaxTokens = 1000,
    //Temperature = 0.7,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(functions: [searchFlights, bookFlight])
};
// Start a chat loop
var history = new ChatHistory();
history.AddSystemMessage("You're flight booking assistant");

AddUserMessage("Find me a flight to Tokyo on the 2026-01-19");
await GetReply();
GetInput();
await GetReply();


void GetInput()
{
    Console.Write("User: ");
    string input = Console.ReadLine()!;
    history.AddUserMessage(input);
}

async Task GetReply()
{
    var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

    ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: settings,
        kernel: kernel
    );
    Console.WriteLine("Assistant: " + reply.ToString());
    history.AddAssistantMessage(reply.ToString());
}

void AddUserMessage(string msg)
{
    Console.WriteLine("User: " + msg);
    history.AddUserMessage(msg);
}


/// <summary>
/// This class is to demonstrate how to create a function invocation filter
/// </summary>
public class PermissionFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        if(!HasUserPermission(context.Function.PluginName, context.Function.Name))
        {
            context.Result = new FunctionResult(context.Result, "The operation was not approved by the user");
            return;
        }
        await next(context);
    }


    private Boolean HasUserPermission(string pluginName, string functionName)
    {
        if (pluginName.Equals("FlightBookingPlugin") && functionName.Equals("book_flight"))
        {
            Console.WriteLine("System Message: The agent requires an approval to complete this operation. Do you approve (Y/N)");
            Console.Write("User: ");
            string shouldProceed = Console.ReadLine()!;

            if (shouldProceed != "Y")
            {
                return false;
            }
        }

        return true;
    }
}